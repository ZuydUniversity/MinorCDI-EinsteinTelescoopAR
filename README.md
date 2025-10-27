# MinorCDI-EinsteinTelescoopAR

De Einstein Telescoop AR-omgeving is ontwikkelt tijdens de minor CDI samen met lectoraat Data Intelligence.

Het project richt zich op het creëren van een AR-ervaring in Unity, waarmee gebruikers op een toegankelijke manier kunnen ontdekken wat de Einstein Telescoop is, hoe deze werkt en waarom de mogelijke bouw in Limburg van groot belang is.


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
cd MinorCDI-EinsteinTelescoopAR
```

3. Voer het volgende commando uit:

```bash
git config core.hooksPath .githooks
```

## Build

1. Open het project in unity.

2. Klik op 'File' en dan 'Build Profiles'.

3. In platforms selecteer 'Andriod XR' en installeer deze als die nog niet geinstalleerd is.

4. Met 'Andriod XR' geselecteerd Klik op 'Switch Platform'.

5. Connect telefoon aan de laptop/pc via een usb-c kabel.

6. Enable 'developer mode' op de telefoon.
    1. Ga naar settings.
    2. Klik op 'About phone' of 'About device'.
    3. Klik op 'Software information'.
    4. Klik 7 keer op 'Build number'.

7. Enable 'USB debugging' op de telefoon.
    1. Ga naar settings.
    2. Klik op 'Developer options'.
    3. Klik op 'USB debugging'.

8. Klik op 'File' en dan 'Build And Run' om de applicatie te starten.
