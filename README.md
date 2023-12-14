### TODO

* Skicka in language om det finns till script som färgar kodblock
* Hantera när chat har för många token
* Se om man kan utvärdera hur många tokens som används
* Kunna välja preprompt
* Spara undan historik
* Söka i historik
* Hantera ctrl-enter så man kan ange flera rader

### Fixat 
* Fixa så att enter-knappen fungerar
* Svar från GPT verkar generera extra radbrytning
* Kolla hur man färgar kodblock
* Bättre färger på chat
* Rensning av chat (ta bort historik)
* Färga sql-kodblock
* Rensa all startup-skräp
* Högerklick-meny för att kopiera text
* Hantera när "bad language"-filter slår till ex: "what books by philip k dick has been filmed" ("Philip K. Dick" verkar fungera)
* Kopiera-knapp på varje kodblock
* Möjlighet att avbryta fråga
* Se till att returnera kopior när settings hämtas
* Fixa settings-sida
* När man går till settings ska chat finnas kvar
* Möjlighet att välja annan model-deployment
* Editera en fråga

## Fixa cert & publicera

### Nytt cert
New-SelfSignedCertificate -Type Custom `
   -Subject "CN=PatrikAlm" `
   -KeyUsage DigitalSignature `
   -FriendlyName "TempDevCert" `
   -CertStoreLocation "Cert:\CurrentUser\My" `
   -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}")

### Kolla mina cert
Get-ChildItem "Cert:\CurrentUser\My" | Format-Table Subject, FriendlyName, Thumbprint

### Fixa cert
Thumbprint ska in i fält <PackageCertificateThumbprint> i csproj-fil

### Paketera för windows
dotnet publish -f net7.0-windows10.0.22621.0 -c Release -p:RuntimeIdentifierOverride=win10-x64