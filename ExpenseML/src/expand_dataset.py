"""Create Indonesian-only, curated training augmentation; never edit baseline splits.

Run: python src/expand_dataset.py
Outputs: data/indonesian_expansion.csv and reports/expansion_comparison.csv
"""
from __future__ import annotations

import csv
from pathlib import Path

import pandas as pd
from sklearn.metrics import accuracy_score, f1_score
from sklearn.pipeline import Pipeline
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.pipeline import FeatureUnion
from sklearn.linear_model import LogisticRegression
from sklearn.svm import LinearSVC

from compare_models import grouped_validation_split

ROOT = Path(__file__).resolve().parents[1]
DATA = ROOT / 'data'
REPORTS = ROOT / 'reports'

# Independent scenarios, not three amount-swapped copies of each sentence.
TRANSACTIONS = {
    'Food': '''beli roti tawar buat sarapan|jajan siomay depan kantor|pesan nasi padang buat makan siang|beli gorengan di warung|makan bakso sepulang kerja|beli telur dan sayur untuk masak|ngopi di kedai dekat rumah|beli seblak pedas|pesan ayam geprek lewat aplikasi|beli buah di pasar|bayar makan malam di warteg|sarapan bubur ayam|beli susu dan sereal|jajan cilok sore ini|beli beras untuk dapur|pesan martabak manis|makan soto ayam|beli air mineral dan roti''',
    'Transport': '''isi bensin motor di spbu|bayar parkir motor di mal|naik ojek online ke kantor|isi saldo kartu tol|bayar tiket kereta komuter|naik bus ke terminal|ongkos angkot pulang kerja|bayar servis rem motor|ganti oli motor di bengkel|bayar taksi ke bandara|isi pertalite untuk perjalanan|bayar parkir mobil harian|beli tiket kapal penyeberangan|bayar cuci motor|ongkos travel antarkota|bayar tambal ban|naik kereta ke bogor|bayar biaya tol perjalanan''',
    'Bills': '''bayar token listrik prabayar|bayar tagihan listrik bulan ini|bayar paket data internet|perpanjang paket telepon bulanan|bayar iuran sampah lingkungan|bayar tagihan air pam|bayar langganan wifi rumah|bayar iuran keamanan komplek|bayar tagihan pascabayar hp|isi ulang pulsa untuk komunikasi|bayar biaya administrasi bank|bayar tagihan gas rumah|bayar langganan internet kabel|bayar tagihan telepon rumah|bayar iuran rt bulanan|bayar tagihan tv kabel|bayar paket internet bulanan|bayar biaya layanan rekening''',
    'Education': '''bayar uang kuliah semester|beli buku pelajaran sekolah|daftar kursus bahasa jepang|bayar les matematika|beli modul persiapan ujian|ikut pelatihan pemrograman|bayar biaya sertifikasi profesi|beli alat tulis untuk kelas|daftar seminar pendidikan|bayar biaya pendaftaran sekolah|beli buku latihan soal|bayar kelas daring desain|ikut bimbingan belajar|beli kamus bahasa indonesia|bayar biaya praktikum kampus|daftar lokakarya menulis|beli buku referensi kuliah|bayar kursus bahasa inggris''',
    'Entertainment': '''main di arena permainan arcade|beli tiket bioskop malam minggu|bayar karaoke bareng teman|langganan layanan film streaming|beli tiket konser musik|main boling di pusat hiburan|bayar tiket taman rekreasi|sewa konsol gim akhir pekan|beli gim baru di toko digital|pergi nonton pertunjukan komedi|bayar tiket kebun binatang|main biliar bersama teman|beli tiket festival musik|bayar wahana permainan air|langganan platform hiburan|nonton pertunjukan teater|bayar tiket museum interaktif|sewa film untuk ditonton''',
    'Health': '''tes darah di laboratorium|bayar konsultasi dokter umum|beli obat flu di apotek|periksa gigi ke dokter|bayar pemeriksaan mata|beli vitamin di apotek|cek kolesterol di klinik|bayar vaksinasi di rumah sakit|periksa kesehatan rutin|bayar rontgen dada|beli obat resep dokter|cek gula darah di laboratorium|bayar fisioterapi lutut|periksa ke dokter kulit|bayar biaya rawat jalan|beli perban dan antiseptik|periksa tekanan darah di klinik|bayar tes kesehatan kerja''',
    'Housing': '''bayar sewa kos bulanan|bayar kontrakan rumah|beli cat untuk dinding kamar|bayar tukang perbaiki atap|beli rak untuk kamar tidur|bayar biaya renovasi dapur|beli kasur baru untuk rumah|bayar jasa bersih bersih rumah|beli lampu untuk ruang tamu|bayar tukang servis pintu|beli tirai jendela kamar|bayar perbaikan pipa bocor|beli meja makan rumah|bayar perbaikan plafon|beli keran air baru|bayar tukang pasang ubin|beli lemari pakaian|bayar jasa pindahan kos''',
    'Shopping': '''beli sepatu olahraga baru|belanja kaos di toko pakaian|beli tas ransel kerja|pesan jaket lewat toko daring|beli kabel pengisi daya|beli pelindung layar ponsel|belanja celana panjang|beli aksesori komputer|pesan mouse nirkabel|beli topi dan ikat pinggang|belanja sandal rumah|beli casing hp baru|beli dompet kulit|pesan keyboard mekanik|beli jam dinding|belanja kemeja kantor|beli payung lipat|pesan botol minum baru''',
    'Business': '''terima pembayaran jasa desain logo|pelanggan bayar pesanan kue|dapat pemasukan dari jualan daring|terima hasil penjualan barang dagangan|klien membayar jasa pembuatan situs|uang masuk dari usaha katering|terima pembayaran proyek usaha|dapat omzet warung hari ini|pelanggan transfer untuk pesanan|hasil penjualan kopi masuk rekening|terima pembayaran jasa fotografi|pemasukan toko dari penjualan harian|dapat bayaran pesanan kerajinan|terima uang dari usaha laundry|hasil jualan makanan hari ini|klien melunasi invoice jasa|terima pendapatan dari toko kecil|dapat pemasukan dari usaha sampingan''',
    'Other Income': '''dapat hadiah uang dari keluarga|terima uang saku dari orang tua|dapat cashback belanja|terima pengembalian dana pesanan|dapat uang angpao|terima hadiah lomba|uang masuk dari pengembalian pinjaman|dapat bonus hadiah undian|terima uang pemberian saudara|dapat pengembalian biaya perjalanan|terima dana hibah pribadi|dapat hadiah ulang tahun berupa uang|terima uang dari penjualan barang pribadi|dapat uang tip dari teman|terima penggantian uang talangan|dapat hadiah uang kompetisi|uang masuk dari pengembalian deposit|dapat transfer hadiah pernikahan''',
    'Salary': '''gaji bulanan sudah masuk rekening|terima upah kerja bulan ini|perusahaan transfer gaji pokok|dapat tunjangan makan dari kantor|terima pembayaran lembur kerja|gajian dari perusahaan hari ini|terima bonus kinerja kantor|dapat tunjangan transport karyawan|gaji november sudah cair|terima tunjangan hari raya kantor|perusahaan bayar insentif pegawai|dapat uang rapel gaji|terima pembayaran honor kerja tetap|gaji bersih masuk bank|dapat bonus tahunan perusahaan|terima tunjangan jabatan|perusahaan kirim gaji bulanan|dapat kompensasi lembur kantor'''
}

OTHER = '''hai asisten keuangan|lagi ngapain sekarang|ceritakan lelucon lucu|aku sedang belajar pemrograman|selamat pagi semuanya|apa kabar hari ini|tolong jelaskan cara kerja aplikasi|bisa bantu aku belajar python|aku sedang lapar|pengen makan nasi goreng nanti|aku suka beli buku|besok mungkin mau belanja|cara masak nasi uduk|aku belum bayar listrik|ingatkan aku untuk beli obat|berapa harga tiket bioskop sekarang|aku penasaran soal tabungan|makan siang enaknya apa|apakah kamu bisa ngobrol|terima kasih sudah membantu|aku sedang di perjalanan|aku mau tidur dulu|bisa jelaskan istilah investasi|uangku lagi menipis|semoga besok gajian|cara membuat anggaran keuangan|aku kepikiran mau beli sepatu|kenapa harga bensin naik|semoga tidur nyenyak|hari ini hujan deras|aku belum jadi belanja|jangan catat transaksi apa pun|batalkan rencana belanja tadi|cuma cerita bukan transaksi|aku ingin menghemat pengeluaran|boleh tanya tentang kesehatan'''
SUMMARY = '''berapa total pengeluaran bulan ini|tampilkan ringkasan pemasukan minggu ini|total uang masuk dan keluar hari ini|berapa pengeluaran makanan bulan lalu|lihat laporan keuangan oktober|berapa pemasukan sepanjang tahun|rekap transaksi kemarin|berapa total belanja pekan ini|tampilkan ringkasan keuangan bulanan|uang keluar paling banyak untuk apa|lihat jumlah transaksi minggu lalu|bandingkan pemasukan dan pengeluaran|berapa total pengeluaran hari ini|tampilkan rekap transaksi bulan berjalan|berapa total pendapatan bulan lalu|lihat laporan pemasukan dan biaya|rekap pengeluaran tiga bulan terakhir|berapa uang yang sudah dibelanjakan|tampilkan pengeluaran berdasarkan kategori|lihat ringkasan transaksi hari ini'''
BUDGET = '''berapa sisa anggaran makan bulan ini|cek batas belanja transport|sisa budget hiburan berapa|apakah anggaran belanja hampir habis|lihat pemakaian anggaran kesehatan|berapa persen budget makanan terpakai|cek anggaran bulanan listrik|apakah pengeluaran melewati batas anggaran|tampilkan sisa dana belanja|berapa jatah belanja bulan ini|lihat batas anggaran pendidikan|sisa anggaran transportasi sekarang|berapa anggaran yang belum terpakai|cek pemakaian budget per kategori|apakah budget kos masih cukup|lihat target anggaran bulanan|berapa sisa budget belanja daring|tampilkan status anggaran hiburan|cek batas biaya kesehatan|berapa dana tersedia dalam anggaran'''


def make_rows():
    rows = []
    for category, blob in TRANSACTIONS.items():
        typ = 'Income' if category in {'Business', 'Other Income', 'Salary'} else 'Expense'
        for i, phrase in enumerate(blob.split('|')):
            rows.append({'text': phrase, 'intent': 'CreateTransaction', 'type': typ,
                         'category': category, 'scenario_group': f'v04_{category.lower().replace(" ", "_")}_{i:02d}'})
    for intent, blob in [('Other', OTHER), ('SummaryQuery', SUMMARY), ('BudgetQuery', BUDGET)]:
        for i, phrase in enumerate(blob.split('|')):
            rows.append({'text': phrase, 'intent': intent, 'type': '', 'category': '',
                         'scenario_group': f'v04_{intent.lower()}_{i:02d}'})
    return pd.DataFrame(rows)


def model_for(task):
    feats = FeatureUnion([
        ('word', TfidfVectorizer(analyzer='word', ngram_range=(1, 2), sublinear_tf=True)),
        ('char', TfidfVectorizer(analyzer='char', ngram_range=(2, 5), sublinear_tf=True)),
    ])
    clf = LogisticRegression(max_iter=2000, class_weight='balanced', random_state=42) if task == 'intent' else LinearSVC(class_weight='balanced', random_state=42, max_iter=5000)
    return Pipeline([('tfidf', feats), ('classifier', clf)])


def main():
    base_path = REPORTS / 'train_split.csv'
    test_path = REPORTS / 'test_split.csv'
    if not base_path.exists() or not test_path.exists():
        raise FileNotFoundError('Run train.py first; original train_split.csv and test_split.csv are required')
    base = pd.read_csv(base_path, dtype=str, keep_default_na=False)
    heldout = pd.read_csv(test_path, dtype=str, keep_default_na=False)
    extra = make_rows()
    if len(extra) != 274:
        raise ValueError(f'Unexpected augmentation size: {len(extra)}')
    for name, df in [('training', base), ('test', heldout), ('expansion', extra)]:
        if df['text'].str.strip().eq('').any() or df['text'].duplicated().any():
            raise ValueError(f'Blank or duplicate text in {name}')
    original = pd.concat([base, heldout], ignore_index=True)
    original_text = set(original['text'].str.casefold().str.strip())
    overlap = extra['text'].str.casefold().str.strip().isin(original_text)
    if overlap.any():
        raise ValueError(f'Expansion duplicates original examples: {extra.loc[overlap, "text"].tolist()}')
    if set(extra['scenario_group']) & set(original['scenario_group']):
        raise ValueError('Expansion scenario_group collides with original dataset')

    fit, val = grouped_validation_split(base)
    DATA.mkdir(parents=True, exist_ok=True)
    REPORTS.mkdir(parents=True, exist_ok=True)
    extra.to_csv(DATA / 'indonesian_expansion.csv', index=False, quoting=csv.QUOTE_MINIMAL)
    print(f'Created {len(extra)} new Indonesian examples across {extra["intent"].nunique()} intents')
    print(f'Original fit={len(fit)}, unchanged validation={len(val)}, held-out test={len(heldout)}')
    results = []
    for task in ('intent', 'category'):
        ycol = task
        fit_task = fit if task == 'intent' else fit[fit['intent'] == 'CreateTransaction']
        val_task = val if task == 'intent' else val[val['intent'] == 'CreateTransaction']
        augmented = pd.concat([fit_task, extra if task == 'intent' else extra[extra['intent'] == 'CreateTransaction']], ignore_index=True)
        for label, train_df in [('baseline', fit_task), ('augmented', augmented)]:
            model = model_for(task)
            model.fit(train_df['text'], train_df[ycol])
            predictions = model.predict(val_task['text'])
            score = f1_score(val_task[ycol], predictions, average='macro', zero_division=0)
            acc = accuracy_score(val_task[ycol], predictions)
            results.append({'task': task, 'training': label, 'train_rows': len(train_df), 'validation_rows': len(val_task), 'accuracy': round(acc, 4), 'macro_f1': round(score, 4)})
            print(f'{task:8} {label:10} accuracy={acc:.3f} macro-F1={score:.3f} train={len(train_df)}')
    pd.DataFrame(results).to_csv(REPORTS / 'expansion_comparison.csv', index=False)
    print('Saved: data/indonesian_expansion.csv, reports/expansion_comparison.csv')
    print('Original dataset, test split, and saved .joblib models were not modified.')


if __name__ == '__main__':
    main()
