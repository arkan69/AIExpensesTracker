"""ExpenseML v0.5: frozen Indonesian challenge and overlap audit.
Run once: python src/challenge_test.py
Does not alter original data, test split, or saved models.
"""
from __future__ import annotations
import json
import re
from pathlib import Path
import pandas as pd
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.metrics.pairwise import cosine_similarity
from sklearn.metrics import accuracy_score, f1_score, classification_report
from expand_dataset import model_for

ROOT = Path(__file__).resolve().parents[1]
DATA, REPORTS = ROOT / 'data', ROOT / 'reports'

# Manually curated challenge scenarios. Each pipe-delimited item is a distinct message.
TRANSACTIONS = {
 'Food': 'tadi siang makan pecel lele habis 28 ribu|sarapan lontong sayur kena 18rb|habis 42k buat ayam bakar|beli tempe tahu buat masak 17.500|pesen dimsum 46 ribuan|jajan es cendol sepulang kerja 12rb|beli daging ayam buat stok dapur 65k|ngemil pisang coklat tadi sore 15 ribu',
 'Transport': 'naik transjakarta tadi kepotong 3500|bayar gojek pulang 23rb|ongkos bensin pertamax 80k|parkir stasiun 8 ribu|tadi bayar grabcar 67.000|isi kartu commuter line 100rb|biaya tambal ban bocor 15k|bayar tiket bus antar kota 145 ribu',
 'Bills': 'tagihan indihome bulan ini 325rb|isi token pln 200 ribu|bayar air pdam 95k|potongan admin rekening 12.500|paket kuota telkomsel 55rb|tagihan kartu halo 110k|bayar iuran keamanan rt 30 ribu|biaya langganan internet rumah 290.000',
 'Education': 'biaya ikut bootcamp data 750rb|bayar spp adik 450 ribu|beli buku algoritma 125k|daftar ujian sertifikasi cloud 1,2 juta|uang les piano anak 300rb|bayar kelas bahasa korea 180k|beli paket latihan utbk 89 ribu|biaya workshop excel 95rb',
 'Entertainment': 'nonton film di cgv 65k|top up game buat beli skin 120rb|beli tiket standup comedy 175 ribu|main timezone habis 70k|sewa lapangan futsal bareng teman 250rb|langganan spotify 55 ribu|tiket masuk dufan 300k|bayar netflix buat hiburan 54rb',
 'Health': 'tebus resep antibiotik di apotek 48rb|konsultasi dokter gigi 250k|bayar cek asam urat 35 ribu|biaya usg di klinik 280rb|beli salep kulit 32k|kontrol ke spesialis mata 190 ribu|bayar tes swab di lab 75rb|biaya suntik vaksin 225k',
 'Housing': 'bayar uang kos november 1,4 juta|servis ac kamar 175rb|beli bantal dan sprei 240k|ongkos tukang benerin wastafel 125 ribu|bayar cicilan sewa apartemen 3 juta|ganti kunci pintu rumah 90rb|beli kursi kerja buat kamar 650k|perbaikan genteng bocor 300rb',
 'Shopping': 'checkout hoodie baru 220rb|beli flashdisk 64gb 85k|pesan charger usb c 125 ribu|belanja kaus kaki 35rb|beli earphone bluetooth 270k|checkout celana jeans 199.000|beli tas selempang 160rb|pesan tempered glass 25k',
 'Business': 'customer lunasin pesanan hampers 850rb|omzet jualan es teh hari ini 420k|klien bayar pembuatan aplikasi 4 juta|pembayaran dari pembeli toko masuk 135rb|hasil jualan keripik 290 ribu|uang jasa edit video dari pelanggan 600k|pemasukan usaha fotokopi 175rb|invoice katering sudah dibayar 2,5jt',
 'Other Income': 'dapat uang kembalian deposit kos 500rb|teman balikin uang yang dipinjam 200k|dapat hadiah lomba foto 750 ribu|refund tiket pesawat masuk 1,2jt|kakak ngasih uang jajan 100rb|cashback promo kartu 45k masuk|terima uang warisan keluarga 3 juta|dapat uang hadiah ulang tahun 250rb',
 'Salary': 'gaji kantor november masuk 8,7jt|uang lembur bulan kemarin cair 450k|dapat thr dari perusahaan 5 juta|insentif pegawai masuk rekening 300rb|honor kerja bulanan cair 4,5 juta|tunjangan jabatan ditransfer 700rb|bonus performa dari kantor 1jt|gajian tanggal 25 sebesar 9 juta',
}
NON_TRANSACTION = {
 'Other': 'kalau beli kopi tiap hari boros gak sih|aku kepikiran mau ganti hp tahun depan|jangan masukkan pengeluaran tadi|bisa jelasin apa itu dana darurat|belum jadi beli tiket ya|aku lagi nunggu gajian nih|tolong jangan simpan transaksi apapun|cara ngatur uang biar gak cepat habis|besok mau cek kesehatan dulu|menurutmu enak makan apa malam ini|aku cuma cerita habis belanja bukan minta dicatat|kalau transfer ke teman aman gak|gimana cara mulai nabung|apakah kamu bisa bantu belajar c sharp|sepertinya harga listrik naik lagi|berapa harga emas sekarang ya',
 'SummaryQuery': 'rekap pengeluaran selama tujuh hari terakhir dong|bulan kemarin uangku habis berapa|total pemasukan dibanding pengeluaran november|tunjukin transaksi minggu ini|berapa kali aku belanja makanan bulan lalu|pengeluaran terbesar tahun ini dari kategori apa|laporan keuangan dari awal bulan sampai sekarang|total uang masuk tiga bulan terakhir|ringkas transaksi yang terjadi kemarin|berapa seluruh biaya transport selama oktober|tampilkan riwayat pengeluaran hari ini|jumlah pendapatan bulan berjalan berapa',
 'BudgetQuery': 'jatah makan bulan ini masih ada berapa|anggaran bensin sudah kepakai berapa persen|sisa plafon belanja bulan november|budget kesehatan masih aman gak|berapa batas biaya hiburan yang tersisa|apakah uang kos sudah melewati anggaran|cek alokasi transport bulan ini|berapa dana yang boleh dipakai buat belanja|anggaran pendidikan sudah habis belum|status pemakaian budget makanan|berapa limit belanja tersisa minggu ini|tunjukin sisa jatah bulanan',
}

def normalize(s):
    return re.sub(r'\s+', ' ', str(s).casefold().strip())

def challenge_rows():
    rows = []
    for category, blob in TRANSACTIONS.items():
        for text in blob.split('|'):
            rows.append({'text': text, 'intent': 'CreateTransaction', 'category': category})
    for intent, blob in NON_TRANSACTION.items():
        for text in blob.split('|'):
            rows.append({'text': text, 'intent': intent, 'category': ''})
    return pd.DataFrame(rows)

def main():
    REPORTS.mkdir(parents=True, exist_ok=True)
    base = pd.read_csv(REPORTS / 'train_split.csv', dtype=str, keep_default_na=False)
    heldout = pd.read_csv(REPORTS / 'test_split.csv', dtype=str, keep_default_na=False)
    extra = pd.read_csv(DATA / 'indonesian_expansion.csv', dtype=str, keep_default_na=False)
    challenge = challenge_rows()
    if len(challenge) != 128 or challenge['text'].map(normalize).duplicated().any():
        raise ValueError('Challenge dataset count/uniqueness unexpected')
    if set(base['scenario_group']) & set(heldout['scenario_group']):
        raise ValueError('Original train/test groups overlap')
    reference = pd.concat([base, heldout, extra], ignore_index=True)
    if set(challenge['text'].map(normalize)) & set(reference['text'].map(normalize)):
        raise ValueError('Exact challenge/reference overlap: review before evaluation')
    # Audit lexical similarity (not proof of semantic independence).
    vectorizer = TfidfVectorizer(analyzer='char', ngram_range=(3, 5))
    matrix = vectorizer.fit_transform(pd.concat([challenge['text'], reference['text']], ignore_index=True))
    similarities = cosine_similarity(matrix[:len(challenge)], matrix[len(challenge):])
    max_idx = similarities.argmax(axis=1)
    challenge['closest_existing_text'] = reference.iloc[max_idx]['text'].to_numpy()
    challenge['max_char_similarity'] = similarities.max(axis=1).round(3)
    challenge.to_csv(REPORTS / 'challenge_similarity_audit.csv', index=False)
    # Save the frozen challenge for reproducibility; never regenerate/change after viewing results.
    challenge[['text', 'intent', 'category']].to_csv(DATA / 'indonesian_challenge_v05.csv', index=False)
    print(f'Challenge: {len(challenge)} independently authored synthetic messages')
    print(f'High lexical similarity >= 0.75: {(challenge.max_char_similarity >= 0.75).sum()} (inspect audit CSV)')
    results, errors = [], []
    for task in ('intent', 'category'):
        label = task
        eval_df = challenge if task == 'intent' else challenge[challenge.intent == 'CreateTransaction']
        fit_base = base if task == 'intent' else base[base.intent == 'CreateTransaction']
        fit_extra = extra if task == 'intent' else extra[extra.intent == 'CreateTransaction']
        for version, fit_df in [('baseline', fit_base), ('augmented', pd.concat([fit_base, fit_extra], ignore_index=True))]:
            model = model_for(task)
            model.fit(fit_df.text, fit_df[label])
            predictions = model.predict(eval_df.text)
            acc = accuracy_score(eval_df[label], predictions)
            f1 = f1_score(eval_df[label], predictions, average='macro', zero_division=0)
            print(f'{task:8s} {version:10s} accuracy={acc:.3f} macro-F1={f1:.3f} challenge={len(eval_df)}')
            results.append({'task': task, 'version': version, 'accuracy': round(float(acc), 4), 'macro_f1': round(float(f1), 4), 'n': len(eval_df), 'classification_report': classification_report(eval_df[label], predictions, output_dict=True, zero_division=0)})
            for message, actual, predicted in zip(eval_df.text, eval_df[label], predictions):
                if actual != predicted:
                    errors.append({'task': task, 'version': version, 'text': message, 'actual': actual, 'predicted': predicted})
    pd.DataFrame(errors, columns=['task','version','text','actual','predicted']).to_csv(REPORTS / 'challenge_errors.csv', index=False)
    (REPORTS / 'challenge_metrics.json').write_text(json.dumps(results, ensure_ascii=False, indent=2), encoding='utf-8')
    print('Saved challenge CSV, similarity audit, errors and metrics. Existing models/test split unchanged.')
    print('Caution: challenge is synthetic and results are exploratory, not production accuracy.')

if __name__ == '__main__':
    main()
