/* Todo-Liste:
 * - KI
 * - Penetrationstests
 * - Entgültigen Gewinner nach Ablauf der Runden anzeigen
 * - Korrekte Zugübermittlung prüfen, potenzielle Fehler!
 * - Memo: Randomseed wieder randomisieren
 * - Gegnerische Yakuübermittlung Prüfen (multiplayer)
 * - Consulting: PC: Austeilen gesammelter Karten
 * - Memo: Gegnerkarten wieder zudecken
 * - Memo: neumischen bei 4 gleichen Karten auf dem Feld
 * - Allwissende KI: 
 *      - Match auf der Hand -> Erst feld, dann einsammeln
 *      - Legt sinnloserweise karten aufs Feld
 *      - ignoriert zwar sinnlose Züge, aber berücksichtigt nicht das Vereiteln gegnerischer kritischer Züge
 *      - Globales Minimum durch Topliste ersetzen (?)
 *      - Wichtig: Overscoring (addpoints)! leicht zu prognostizieren (Kasu = 24/10 = 14Punkte!)
 * - Priorität bei allwissender KI:
 *      1. Globales Minimum
 *      2. Lokales Minimum
 *      2. Vereiteln gegnerischer Züge (+)
 *      3. Warten auf bessere Karte
 *      4. Vermeiden von "In die Hand spielen"
 *      5. Einsammeln durch passen
 *      6. Passen
 * 
 * Zurückgestellt:
 * - Win/Loose Animationen
 * - (Spielverlauf visualisieren)
 * - (Sammlung bei PC-Version überarbeiten)
 * - (Überarbeitung der PC- und Mobil-UI)
 * - Memo, Separation von Handzug und Deckzug!!!!!!!!!!!!, Spielzug als Tupel

*/