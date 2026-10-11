# Hangman Pro (WPF / C#)

Ez a projekt a klasszikus Akasztófa játék modernizált, C# és WPF (Windows Presentation Foundation) alapú asztali alkalmazás változata. 

## A projekt leírása és a fejlesztések
A kezdeti verzióhoz képest az alkalmazás jelentős vizuális és funkcionális frissítéseken esett át, hogy egy sokkal modernebb és élvezetesebb felhasználói élményt (UX) nyújtson.

### Kiemelt módosítások és funkciók:
- **Modern UI/UX dizájn:** A korábbi egyszerű szürke felületet egy sötétkék alapú, türkiz kiemelésekkel rendelkező modern dizájn váltotta fel. Újratervezett kezdőképernyő és látványos grafikai elemek (pl. lekerekített gombok).
- **Átlátható panelek:** A játékterület különálló panelekre lett osztva (akasztófa grafikája, keresett szó, kezelőgombok) a letisztult elrendezés érdekében.
- **Kibővített szókészlet:** Teljesen átdolgozott és kibővített szólisták (kategóriánként 25 szó).
- **Új időkezelés:** Rugalmas játékmódok (3, 5, 10 perces limit, vagy időkorlát nélküli játék).
- **Továbbfejlesztett statisztikák:** Jól látható, különálló kártyákon jelenik meg a pontszám, a hátralévő idő és a hibák száma.
- **Magyarított billentyűzet:** A virtuális billentyűzet kiegészült a magyar nyelv specifikus karaktereivel (ékezetes betűk, és két/háromjegyű mássalhangzók: sz, cs, ny, gy stb.).
- **Reszponzív ablak:** Fixált (dinamikus) felületi igazítások, hogy az ablak átméretezésekor is stabil maradjon az elrendezés.

## Használt technológiák
- **Nyelv:** C#
- **Keretrendszer:** .NET / WPF (Windows Presentation Foundation)
- **Architektúra:** Core játéklogika, XAML adatkötések (Data Binding)

## Jövőbeli tervek (Kompatibilitás)
A projekt sikeresen átesett egy mobil-optimalizálási folyamaton is, így a megfelelő környezetben érintőképernyős eszközökön is futtatható.
