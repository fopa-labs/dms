# Sprintterv

Kéthetes sprintek, sprintenként kb. 25 munkaóra (heti 12–13 óra). Az M0–M5 14 sprint, kb. 7 hónap; ez az interjún bemutatható állapot. A feladatok mellett: **(AI)** = AI-ra bízható, **(saját)** = magad írod (lásd CLAUDE.md, „Learning boundaries”). A végrehajtás követése GitHub issue-kban és milestone-okban történik; ez a fájl a terv.

## Ütemezés

| Mérföldkő | Sprintek |
| --- | --- |
| M0 – Váz | S1 |
| M1 – DMS-mag | S2–S3 |
| M2 – Feldolgozás | S4–S6 (szükség esetén S6b) |
| M3 – Eval, keresés | S7–S8 |
| M4 – Embedding | S9–S11 |
| M5 – Agent | S12–S14 — **interjúkész állapot** |
| M6 – Ábrák | S15–S16 |
| M7 – Járműkontextus | S17 |
| M8 – Általános dokumentumok | S18–S21 |
| M9 – Évkönyvek | S22–S23 |
| M10 – Bekötési gráf (opcionális) | S24+ |

## M0–M1: váz és DMS-mag (S1–S3)

| Sprint | Cél | Feladatok | Kész, ha |
| --- | --- | --- | --- |
| S1 | Váz, minőségi alapok | Solution-struktúra, `Directory.Build.props`, analyzerek, Central Package Management, `.editorconfig` (AI) · Aspire AppHost pgvectoros Postgresszel (AI) · GitHub Actions build, secret scanning, `.gitignore` (AI) · `CLAUDE.md` a konvenciókkal (saját) · EF Core entitások és első migráció (AI, átnézve) · Identity cookie-auth egy felhasználóval (saját) · Testcontainers-fixture (AI) és egy integrációs teszt (saját) · architektúra-teszt · első két ADR (saját) | `dotnet run` az AppHoston elindítja a stacket; a CI zöld; bejelentkezés működik |
| S2 | Feltöltés és tárolás | Gyűjtemény-végpontok (saját) · streaming multipart feltöltés pufferelés nélkül (saját) · `IBlobStore` helyi implementáció, SHA-256 dedup (saját) · méretkorlát, tartalom alapú típusellenőrzés, útvonal-védelem (saját) · OpenAPI + Scalar (AI) · integrációs tesztek 1 GB-os fájlra is (saját) | Egy több száz MB-os PDF feltölthető állandó memóriahasználattal; duplikátum nem tárolódik kétszer |
| S3 | DMS-funkciók és UI | Verziók, aktív verzió, címkék, `jsonb` metaadat, puha törlés, audit (saját) · kurzoros lapozás, ETag / `If-Match` (saját) · health check-ek, rate limiter (AI) · dokumentumlista és feltöltő nézet vanilla JS-ben (AI) · README első változata (AI vázlat, saját véglegesítés) · `SECURITY.md` első változata | A UI-on feltölthető, listázható, címkézhető, verziózható egy dokumentum; **a repó már bemutatható** |

## M2: feldolgozás (S4–S6)

A PdfPig-kockázat miatt itt a legnagyobb a szórás; ha az S5 minőségellenőrzése rossz eredményt ad, egy negyedik sprint (S6b) is kellhet.

| Sprint | Cél | Feladatok | Kész, ha |
| --- | --- | --- | --- |
| S4 | Feladatsor és pipeline-váz | Worker Service projekt (AI váz) · `job`-sor a specifikáció állapotgépe szerint: `FOR UPDATE SKIP LOCKED`, bérlet, heartbeat, újrapróbálás, dead-letter, idempotencia, zombi Worker elleni védelem (saját) · egységes blokkmodell és az öt pipeline-interfész (saját) · `register` job, `run_id`, számláló (saját) · feldolgozási állapot SSE-n (saját) · tesztek: párhuzamos workerek, leállás közbeni job (saját) | Feltöltés után a job lefut, állapota a UI-on látszik; worker-leállítás után a job folytatódik |
| S5 | Kinyerés és osztályozás | PdfPig-kinyerés egy 50 oldalas mintán, minőségellenőrzés jegyzőkönyvvel (saját) · `classify_pages` C#-portja (AI) és kalibrálása 100–200 címkézett oldalon (saját) · `page`, `block`, `page_label` feltöltése; fejléc/lábléc felismerése (saját) · ADR a kinyerés minőségéről (saját) | Az osztályozó pontossága mérve a kézi címkéken; a rajzfeliratok koordinátákkal az adatbázisban |
| S6 | Renderelés, darabolás, kódok | Oldalrenderelés PDFium-alapú könyvtárral (AI) · könyvjelző-alapú `IChunker` a modell maximális hosszán belül (saját) · Mazda-enricher regexei (AI) és tesztjei (AI, átnézve) · `section_link` (saját) · tömeges írás `COPY`-val (saját) · teljes kézikönyv feldolgozása, futásidő mérve (saját) | Mind a 13 000 oldal feldolgozva; chunkok, kódok, oldalképek a helyükön |

## M3: eval és lexikális keresés (S7–S8)

| Sprint | Cél | Feladatok | Kész, ha |
| --- | --- | --- | --- |
| S7 | Keresés | Kódkeresés a `chunk_code`-on, pontos és trigram egyezéssel (saját) · `tsvector`-keresés gyűjteményenkénti `ts_config`-gal, `websearch_to_tsquery` (saját) · indexek, `EXPLAIN ANALYZE`-mérés, ADR (saját) · `/api/search` és keresőmező a UI-on (UI: AI) | DTC-kódra pontos találat; a lekérdezési terv indexet használ |
| S8 | Kiértékelés | 40 + 4 kérdés a kézikönyvhöz, helyes oldalakkal, a repón kívül (saját, 8–15 óra) · demo-korpusz és saját kérdései (korpusz: AI, kérdések: saját) · `tools/Dms.Eval`, Recall@5/10, MRR, `eval_run`, összevető riport (saját) · CI-lépés küszöbbel a demo-korpuszon (AI) · eredménytábla a README-ben (saját) | Lexikális alapvonal mérve; a CI elbukik, ha a recall a küszöb alá esik |

## M4: embedding (S9–S11)

| Sprint | Cél | Feladatok | Kész, ha |
| --- | --- | --- | --- |
| S9 | ONNX-pipeline | Modell-letöltő szkript hash-ellenőrzéssel (AI) · tokenizálás `Microsoft.ML.Tokenizers`-szel, batch-képzés, padding, maszk (saját) · ONNX Runtime inferencia, mean pooling, L2-normalizálás (saját) · referenciavektorok egy hivatalos implementációból, egyezés-teszt tűréssel (teszt: saját) | A saját vektorok a referenciával koszinuszban gyakorlatilag egyeznek |
| S10 | Indexelés és hibrid keresés | pgvector-oszlop, `active` / `collection_id`, részleges HNSW-index, `IVectorIndex` (saját) · `embed` job, vektorírás egy tranzakcióban, `indexed_at` (saját) · `finalize` job, feltételes élesítés (saját) · RRF-fúzió a három ágra (saját) · teljes kézikönyv beindexelése a fejlesztői gépen, idő mérve (saját) · eval az alapvonallal szemben (saját) | A hibrid keresés Recall@10-e mérve, és összevetve a lexikális alapvonallal |
| S11 | Kísérletek | `e5-small` / `e5-base` / `bge-m3` összevetés · int8-kvantálás · chunkméret és átfedés · magyar kérdés vs. angol keresőkifejezés · HNSW-paraméterek, szűrt keresés · opcionálisan Qdrant második `IVectorIndex`-implementációként, összemérve · Gemini embedding referenciaként az `IEmbeddingGenerator` mögött (bekötés: AI) · BenchmarkDotNet (váz: AI, mérés és értelmezés: saját) · eredménygrafikonok és ADR (saját) | Végleges modell- és chunkméret-döntés méréssel alátámasztva, a README-ben publikálva |

## M5: agent (S12–S14)

Az S12 előtt ellenőrizni kell a Vertexen a Claude-modellek régióját, a kvótát és a prompt-gyorsítótár támogatását.

| Sprint | Cél | Feladatok | Kész, ha |
| --- | --- | --- | --- |
| S12 | Modellkliens és agent-ciklus | Vertex-kliens service account hitelesítéssel, `streamRawPredict`, stream-feldolgozás (saját) · resilience pipeline (AI) · tool-use ciklus korlátokkal (15 hívás, 120 s, 4 kép) (saját) · `list_collections`, `search`, `get_chunk`, `get_toc`, `lookup_code` (saját) · rögzített válaszokat adó teszt-`IChatClient` (AI) | Konzolos tesztben az agent kézikönyvi hivatkozással válaszol egy DTC-kérdésre |
| S13 | Beszélgetés és UI | `conversation` / `message` tárolás tokenhasználattal (saját) · SSE-végpont az eseménytípusokkal (saját) · régi eszközválaszok tömörítése (saját) · hivatkozások szerveroldali ellenőrzése (saját) · chat-nézet, összecsukható eszközhívások, forráshivatkozás → oldalnézet (AI) · rendszerutasítás a specifikáció szabályaival (saját) | A UI-on többlépéses diagnosztikai párbeszéd folytatható, kattintható, ellenőrzött forrásokkal |
| S14 | Biztonság, megfigyelhetőség, kiadás | OpenTelemetry-spanek eszközhívásonként, token-metrikák (saját) · prompt injection védelem és tesztek „mérgezett” dokumentumokkal (dokumentumok: AI, védelem és tesztek: saját) · válaszminőség-eval LLM-bírálóval (saját) · `SECURITY.md` bővítése · README: GIF, Aspire-dashboard kép, eredmények; ADR-ek kiegészítése (saját) · `v0.5` release | **Interjúkész állapot:** az M0–M5 teljes, dokumentált, mért |

## M6–M10: durvább bontás (S15–S23+)

Ezeket az M5 lezárásakor érdemes részletezni, az addigi mérések és tapasztalatok alapján.

| Sprint | Mérföldkő | Fő tartalom |
| --- | --- | --- |
| S15–S16 | M6 – Ábrák | `get_page_image` régióval és kiemeléssel a feliratkoordináták alapján (saját) · `describe_figure` job vision-modellel, gyorsítótárral (bekötés: AI) · `render_diagram` önjavító hibaciklussal (saját) |
| S17 | M7 – Járműkontextus | Járműprofil, szervizelőzmény megerősítéses írással (saját) · FORScan CSV-elemző (AI) |
| S18–S21 | M8 – Általános dokumentumok | Tesseract-alapú `IOcrEngine`, oldalankénti OCR vegyes PDF-re (saját) · képfájlok · DOCX/XLSX/HTML/EML-kinyerők (AI, átnézve) · címsor- és tokenszám-alapú chunker (saját) · nyelvfelismerés · eval vegyes dokumentumokra |
| S22–S23 | M9 – Évkönyvek | Szkennelés/beszerzés, OCR-minőség mérése, magyar `ts_config`, gyűjteményválasztás az agentben |
| S24+ | M10 – Bekötési gráf (opcionális) | Kutatás jellegű, 3–6 sprint; egyetlen rajzon kezdve, kézzel ellenőrzött bekötéssel mérve |
