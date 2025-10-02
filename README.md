# MinorCDI-EinsteinTelescoopAR

Een AR applicatie die uitleg geeft over wat de einstein telescoop is, zijn werking en de invloed die deze heeft.

## Benodigheden

Zorg dat je het volgende op je systeem geinstalleerd hebt:

* git
* Unity (https://unity.com/download)

## Setup

1. Clone de repository:

```bash
git clone git@github.com:ZuydUniversity/MinorCDI-EinsteinTelescoopAR.git
```

2. Ga naar de directory:

```bash
cd EinsteinTelescoopAR
```

3. Voer het volgende commando uit:

```bash
git config core.hooksPath .githooks
```

## Build

1. Ga naar de Server folder:

```bash
cd Server
```

2. Zet de venv op:

```bash
python -m venv venv
```

3. Open venv:

```bash
venv\Scripts\activate
```

4. Installer de packages:

```bash
pip install -r requirements.txt
```

4. Start de server:

```python
python server.py
```