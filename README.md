# Hanafuda

Hanafuda ist ein klassisch japanisches Kartenspiel. In diesem Projekt wird die Spielweise "Koi Koi" praktiziert.
Dieses Projekt verwirklicht dies in einem 3D-Spiel, dass auch auf mobilen Plattformen unterstützt werden soll,
es bau außerdem einen Einzel- und Mehrspielermodus ein.

## Voreinstellungen

Im Projekt werden 2 Schwerpunkte angegangen: Multiplayer und KI. Im Mehrspielermodus kann man über Namen und andere
Einstellungen nach einem Spieler suchen oder sich selbst suchen lassen.
Der Einzelspielermodus wird 3 Typen von KI bieten:
- Einfacher Modus: Selbe Informationen wie Spieler, Wahrscheinlichkeitsberechnung der Folgezüge
- Schwieriger Modus: Aufbau eines Zustandsbaumes um den bestmöglichen Endzustand zu erreichen
- Alptraum Modus: KI kennt alle zugedeckten Karten und berechnet Züge über Bewertungsfunktion

Weiterhin kann man zwischen 6 und 12 Runden pro Spiel wählen.

## Die Spielkarten

Das Deck besteht aus 4x12=48 Karten. 4 Karten vertreten jeweils einen der zwölf Monate, gleichzeitig verkörpern sie bestimmte
Motive, die in vier Kategorien eingeteilt werden: Tiere, Landschaft, Bänder und Lichter. Außerdem haben die Karten bestimmte
Namen, die in einigen Fällen für den Spielverlauf entscheidend sind.

Die genauen Kartendefinitionen können im Ordner [Assets/Resources/Deck](Assets/Resources/Deck) gefunden werden.

Die Kartenbilder können im Ordner [Assets/Resources/Motive](Assets/Resources/Motive) gefunden werden.

## Spielregeln
Bevor das Spiel beginnt wird der Oya ausgehandelt. Dazu werden 12 Karten, 
die die verschiedenen Monate symbolisieren zugedeckt zur Wahl gegeben.
Jeder Spieler wählt eine Karte und der mit dem früheren Monat darf das Spiel beginnen.

Im Hauptteil des Spiels bekommt jeder Spieler 8 Karten auf die Hand, 8 werdem dem Feld aufgedeckt ausgeteilt.
Nun ist es die Aufgabe jedes Spielers, Karten, die denselben Monat abbilden, mit dem Feld zu Paaren und einzusammeln.
Sind auf dem Feld zwei gleiche Karten vorhanden, darf nur eine davon eingesammelt werden, ansonsten alle.
Ist keine Übereinstimmung vorhanden wird die ausgewählte Karte aufs Feld gelegt.
Am Ende eines jeden Zuges zieht der Spieler eine Karte vom Deck auf das Feld. Auch hier gelten die selben Regeln,
wie beim Zug von der Hand. Daraus ergibt sich eine Höchstzahl von 8 Zügen pro Runde.

Das Spiel kann beendet werden, wenn ein Spieler einen sog. Yaku geformt hat. Das sind Anforderungen an die eingesammelten
Karten, die je eine bestimmte Menge an Punkten geben und je nach Typ erweitert werden können. So kann z.B. der Yaku "Kasu"
jedes mal erneut aufgerufen werden, solange man eine Karte vom Typ "Landschaften" einsammelt. 
Dies gibt jedes Mal einen weiteren Punkt. Hat man einen dieser Yaku gesammelt besteht die Möglichkeit "Koi Koi" zu sagen.
Tut man dies nicht ist das Spiel beendet. Nur der Spieler, der dies tut bekommt seine Punkte gutgeschrieben. Alle anderen 
gesammelten Punkte gehen verloren. Sagt man "Koi Koi" erhält man zusätzliche Punkte und kann weiterspielen.

Das Spiel gewinnt, wer nach der festgelegten Rundenzahl von 6 oder 12 Runden die meisten Punkte hat.

## Yaku

Wie bereits angedeutet, sind Yaku bestimmte Sammlungen von Karten, die ein bestimmtes Kriterium erfüllen müssen.
Dies kann eine bestimmte Art Motiv sein, konkrete Namen oder eine Kombination aus Beidem sein. Jeder Yaku gibt dem Spieler
eine bestimmte Punktzahl. Allerdings bauen einige von ihnen aufeinander auf. So kann z.B. nur entweder der Yaku
"Akatan", "Aotan" oder "Aka Ao Kasane" gesammelt werden. Dasselbe gilt für die Lichter-Yaku. Andere geben immer wieder Punkte,
wenn eine Karte eingesetzt wird. Ein Beispiel dafür ist der "Kasu"-Yaku. Ab 10 Karten des Typs "Landschaft" gibt er für jede
weitere Karte einen Punkt. 12 Karten ergeben 3 Punkte, 4 Karten 4, usw...

Yakudefinitionen können im Ordner [Assets/Resources/Yaku](Assets/Resources/Yaku) gefunden werden.
