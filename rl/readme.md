# Installation (Windows)
Python 3.10.11
https://www.python.org/downloads/release/python-31011/

fixed by: https://github.com/Unity-Technologies/ml-agents/issues/6008

- create venv

- git clone --branch release_21 https://github.com/Unity-Technologies/ml-agents.git
- in `ml-agents-env/setup.py` change numpy version to `numpy==1.23.3`
- then:
```
cd /path/to/ml-agents
python -m pip install ./ml-agents-envs
python -m pip install ./ml-agents
```

- (optional) might need to install cuda enabled torch
`pip3 install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/cu118`

- `pip freeze` packages should look like:
```
absl-py==2.1.0
attrs==23.2.0
cattrs==1.5.0
certifi==2024.2.2
charset-normalizer==3.3.2
cloudpickle==3.0.0
colorama==0.4.6
filelock==3.14.0
fsspec==2024.5.0
grpcio==1.48.2
gym==0.26.2
gym-notices==0.0.8
h5py==3.11.0
huggingface-hub==0.23.1
idna==3.7
intel-openmp==2021.4.0
Jinja2==3.1.4
Markdown==3.6
MarkupSafe==2.1.5
mkl==2021.4.0
mlagents==1.0.0
mlagents-envs==1.0.0
mpmath==1.3.0
networkx==3.3
numpy==1.23.3
onnx==1.12.0
packaging==24.0
PettingZoo==1.15.0
pillow==10.3.0
protobuf==3.19.6
pypiwin32==223
pywin32==306
PyYAML==6.0.1
requests==2.32.2
six==1.16.0
sympy==1.12
tbb==2021.12.0
tensorboard==2.16.2
tensorboard-data-server==0.7.2
torch==2.3.0+cu118
torchaudio==2.3.0+cu118
torchvision==0.18.0+cu118
tqdm==4.66.4
typing_extensions==4.11.0
urllib3==2.2.1
Werkzeug==3.0.3
```