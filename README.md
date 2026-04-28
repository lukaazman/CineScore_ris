## **PROBLEMSKA DOMENA CINESCORE**
#### **Student**: Luka Ažman (63230009) in Teodor Todorović (63240328)
#### **Naslov**: CineScore – sistem za ocenjevanje filmov


### Opis
CineScore je informacijski sistem namenjen pregledovanju, ocenjevanju in organiziranju filmov. Sistem tudi omogoča iskanje filmov po naslovih. Uporabniki lahko filme ocenjujejo, komentirajo ter ustvarjajo lastne sezname priljubljenih filmov ali filmov, ki si jih želijo ogledati v prihodnosti.
Sistem je sestavljen iz uporabniškega dela in administratorskega dela. Uporabniški del omogoča registriranim uporabnikom pregled filmov, ocenjevanje, komentiranje ter upravljanje osebnega profila. Poleg tega lahko uporabniki dodajajo filme na seznam za kasnejši ogled (Watchlist). Neregistrirani uporabniki lahko filme pregledujejo, vendar ne morejo dodajati ocen ali komentarjev.
Administratorski del sistema je namenjen upravljanju vsebine aplikacije. Administratorji lahko dodajajo nove filme, urejajo obstoječe podatke o filmih ter upravljajo uporabniške račune. Sistem uporablja podatkovno bazo za shranjevanje informacij o filmih, ocenah, komentarjih in uporabnikih. Vsak film v sistemu vsebuje osnovne podatke, kot so naslov, leto izdaje, žanr, opis in povezavo do slike ali posterja filma. Registrirani uporabniki lahko filmu dodelijo oceno in napišejo komentar. Sistem na podlagi vseh ocen izračuna povprečno oceno filma, ki je prikazana vsem uporabnikom. Poleg tega sistem omogoča tudi odzive na komentarje (Like/Dislike).


### Funkcionalnosti:
• Registracija novega uporabnika  
• Prijava uporabnika v sistem  
• Pregled in iskanje filmov po naslovih  
• Ocenjevanje filmov  
• Dodajanje komentarjev k filmom  
• Dodajanje filmov na seznam za kasnejši ogled (Watchlist)  
• Like/Dislike na komentarje drugih uporabnikov  
• Upravljanje filmov (dodajanje, urejanje in brisanje) – administrator  
• Upravljanje uporabniških računov – administrator


### Opis toka dogodkov:
• Uporabnik odpre spletno stran sistema CineScore.  
• Sistem prikaže seznam filmov in možnost prijave ali registracije.  
• Uporabnik se prijavi v sistem.  
• Sistem preveri uporabniške podatke v podatkovni bazi.  
• Po uspešni prijavi sistem prikaže uporabniški profil in seznam filmov.  
• Uporabnik izbere film.  
• Sistem prikaže podrobnosti filma (opis, žanr, leto izdaje, ocene in komentarje).  
• Uporabnik lahko odda oceno, napiše komentar ali doda film na seznam za kasnejši ogled.  
• Sistem shrani podatke v podatkovno bazo in posodobi prikaz ocen.

### Alternativni tok dogodkov:
• Uporabnik poskuša oddati oceno brez prijave.  
• Sistem uporabnika obvesti, da mora biti za ocenjevanje prijavljen.  
ali  
• Uporabnik poskuša dodati film na seznam priljubljenih, ki je že na seznamu.  
• Sistem prikaže obvestilo, da je film že dodan na seznam.  

#### Primer uporabnika:
*e-pošta*: `demo@example.com`
*geslo*: `Demo123!`
