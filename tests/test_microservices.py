import io
import uuid
import pytest
import requests

FILE_STORE_URL    = "http://localhost:5002/api/file"
FILE_ANALYSIS_URL = "http://localhost:5004/api/fileinfo"
GATEWAY_UPLOAD    = "http://localhost:6000/api/file/upload"
GATEWAY_DOWNLOAD  = "http://localhost:6000/api/file/upload"
GATEWAY_ANALYSIS  = "http://localhost:6000/api/file/analysis"

@pytest.fixture
def sample_txt(tmp_path):
    p = tmp_path / "hello.txt"
    p.write_text("Line1\n\nLine2 Word")
    return p

@pytest.fixture
def complex_txt(tmp_path):
    content = "Para1\tword \r\n\r\nPara2  with   multiple spaces\n\nFinal"
    p = tmp_path / "complex.txt"
    p.write_text(content)
    return p

def test_store_and_retrieve_direct(sample_txt):
    with open(sample_txt, 'rb') as f:
        r = requests.post(FILE_STORE_URL, files={'file': f})
    assert r.status_code == 200
    file_id = r.json()['id']
    r2 = requests.get(f"{FILE_STORE_URL}/{file_id}")
    assert r2.status_code == 200
    assert r2.text == sample_txt.read_text()

def test_analysis_counts_direct(sample_txt, complex_txt):
    for txt in (sample_txt, complex_txt):
        with open(txt, 'rb') as f:
            r = requests.post(FILE_STORE_URL, files={'file': f})
        fid = r.json()['id']
        info = requests.get(f"{FILE_ANALYSIS_URL}/{fid}").json()
        if txt == sample_txt:
            assert info['paragraphCount'] == 2
            assert info['wordCount'] == 3
        else:
            assert info['paragraphCount'] == 3
            assert info['wordCount'] == 7
        assert info['characterCount'] >= len(txt.read_text())

def test_gateway_end_to_end(sample_txt):
    with open(sample_txt, 'rb') as f:
        r = requests.post(GATEWAY_UPLOAD, files={'file': f})
    assert r.status_code == 200
    gid = r.json()['id']
    r2 = requests.get(f"{GATEWAY_DOWNLOAD}/{gid}")
    assert r2.status_code == 200
    assert r2.text == sample_txt.read_text()
    r3 = requests.get(f"{GATEWAY_ANALYSIS}/{gid}")
    j = r3.json()
    assert j['paragraphCount'] == 2
    assert j['wordCount'] == 3

def test_gateway_upload_wrong_extension():
    files = {'file': ('foo.jpg', io.BytesIO(b'abc'))}
    r = requests.post(GATEWAY_UPLOAD, files=files)
    assert r.status_code == 400

def test_gateway_missing_file_param():
    r = requests.post(GATEWAY_UPLOAD)
    assert r.status_code == 400

def test_download_invalid_guid_format_via_gateway():
    r = requests.get(f"{GATEWAY_DOWNLOAD}/not-a-guid")
    assert r.status_code == 404

def test_analysis_invalid_guid_format_via_gateway():
    r = requests.get(f"{GATEWAY_ANALYSIS}/1234")
    assert r.status_code == 404

def test_not_found_store_and_analysis():
    fake_id = uuid.uuid4()
    r1 = requests.get(f"{FILE_STORE_URL}/{fake_id}")
    assert r1.status_code == 404
    r2 = requests.get(f"{FILE_ANALYSIS_URL}/{fake_id}")
    assert r2.status_code == 404

def test_plagiarism_flag(tmp_path):
    content = f"Unique content {uuid.uuid4()}"
    p = tmp_path / "unique.txt"
    p.write_text(content)
    with open(p, 'rb') as f:
        id1 = requests.post(FILE_STORE_URL, files={'file': f}).json()['id']
    with open(p, 'rb') as f:
        id2 = requests.post(FILE_STORE_URL, files={'file': f}).json()['id']
    info1 = requests.get(f"{FILE_ANALYSIS_URL}/{id1}").json()
    assert info1['isPlagiarized'] is False
    info2 = requests.get(f"{FILE_ANALYSIS_URL}/{id2}").json()
    assert info2['isPlagiarized'] is True

def test_gateway_plagiarism_via_gateway(tmp_path):
    content = f"Another content {uuid.uuid4()}"
    p = tmp_path / "another.txt"
    p.write_text(content)
    with open(p, 'rb') as f:
        gid1 = requests.post(GATEWAY_UPLOAD, files={'file': f}).json()['id']
    with open(p, 'rb') as f:
        gid2 = requests.post(GATEWAY_UPLOAD, files={'file': f}).json()['id']
    info1 = requests.get(f"{GATEWAY_ANALYSIS}/{gid1}").json()
    assert info1['isPlagiarized'] is False
    info2 = requests.get(f"{GATEWAY_ANALYSIS}/{gid2}").json()
    assert info2['isPlagiarized'] is True

def test_consistency_direct_vs_gateway_analysis(sample_txt):
    with open(sample_txt, 'rb') as f:
        fid = requests.post(FILE_STORE_URL, files={'file': f}).json()['id']
    direct = requests.get(f"{FILE_ANALYSIS_URL}/{fid}").json()
    via = requests.get(f"{GATEWAY_ANALYSIS}/{fid}").json()
    assert direct == via

def test_many_uploads_unique_ids(sample_txt):
    ids = set()
    for _ in range(5):
        with open(sample_txt, 'rb') as f:
            fid = requests.post(FILE_STORE_URL, files={'file': f}).json()['id']
        assert fid not in ids
        ids.add(fid)

def test_delete_and_reupload(tmp_path):
    p = tmp_path / "temp.txt"
    p.write_text("Hello")
    with open(p, 'rb') as f:
        fid = requests.post(FILE_STORE_URL, files={'file': f}).json()['id']
    _ = requests.get(f"{FILE_ANALYSIS_URL}/{fid}")
    r = requests.get(f"{FILE_STORE_URL}/{fid}")
    assert r.status_code == 200