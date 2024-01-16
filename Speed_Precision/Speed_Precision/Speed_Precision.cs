using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;
using Silk.NET.Core.Native;


namespace Speed_Precision
{
    /// @author Haukjohe
    /// @version 23.03.2023
    /// <summary>
    /// Luodaan Speed and precision peli jossa tuhotaan sinisia nelioita.
    /// </summary>
    public class Speed_Precision : PhysicsGame
    {
        /// <summary>
        /// Jouni on vihaninen kuva.
        /// </summary>
        private Image jouniVihanen = LoadImage("Jounivihane");
        /// <summary>
        ///Jouni on neutraali kuva.
        /// </summary>
        private Image jouniNeutraali = LoadImage("JouniNeutraali");
        /// <summary>
        ///Jouni on iloinen kuva.
        /// </summary>
        private Image jouniilonen = LoadImage("Jouniilonen");
        /// <summary>
        ///Hiscore lista, johon tallentuu hiscoret.
        /// </summary>
        private EasyHighScore topLista = new EasyHighScore();
        /// <summary>
        ///Fysiikkaobjekti jounille.
        /// </summary>
        private PhysicsObject jouni;


        /// <summary>
        /// Aliohjelmalla käynnistetaan koko pelimaailma ja muutetaan kuvien scaling paremmaksi.
        /// </summary>
        public override void Begin()
        {
            Luokentta();
            jouniilonen.Scaling = ImageScaling.Nearest;
            jouniVihanen.Scaling = ImageScaling.Nearest;
            jouniNeutraali.Scaling = ImageScaling.Nearest;
        }


        /// <summary>
        /// Aliohjelmalla luodaan pelikentta ja initialisoidaan
        /// pistelaskuri ja kutsutaan siihen kuuluvia funktioita. Myös aikalaskuri ja vihainen Jouni kutsutaan tässä.
        /// </summary>
        public void Luokentta()
        {
            Level.Size = Screen.Size;
            Level.BackgroundColor = Color.Black;
            Mouse.IsCursorVisible = true;
            IntMeter pisteLaskuri = new IntMeter(0);
            LuoPisteLaskuri(pisteLaskuri);
            LuoAikaLaskuri(pisteLaskuri, 10, 100);
            LuoJouni(jouniVihanen);
            pelinPyoritys(pisteLaskuri);

        }


        /// <summary>
        /// Tehdään Jouni hymiö, jossa parametrinä pyydetää itse kuvaa, ja funktiossa annetaan kuvalle paikka 
        /// ja lisätään kuva näytölle.
        /// </summary>
        /// <param name="kuva">mika kuva tuodaan</param>
        public void LuoJouni(Image kuva)
        {
            jouni = new PhysicsObject(140, 140);
            jouni.Y = Screen.Bottom + 400;
            jouni.X = Screen.Right - 130;

            jouni.Image= kuva;
            Add(jouni);
        }


        /// <summary>
        /// Aliohjelma pyorittaa pelia, eli kun halutaan tehdä uusi taso, niin talla aliohjelmalla
        /// se tapahtuu. Ensin luodaan uudet neliot ja castataan niihin arvoja PiirraGrid funktiolla,
        /// sen jalkeen kuunnellaan hiirta etta missa kohtaa se painaa left click.
        /// </summary>
        /// <param name="pisteLaskuri">pistelaskuri tuodaan mukana</param>
        public void pelinPyoritys(IntMeter pisteLaskuri)
        {
            List<PhysicsObject> neliot = new List<PhysicsObject> { };
            neliot = PiirraGrid(neliot);
            KuunteleHiirta(neliot, pisteLaskuri);
            Keyboard.Listen(Key.F5, ButtonState.Pressed, AloitaAlusta, "Uusi peli");
            Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");

        }


        /// <summary>
        /// Aliohjelmalla Piirretaan pelikentta. Eli talla hetkella 3x3 boxit, joista 3kpl on sinisia 
        /// ja loput valkoisia.
        /// </summary>
        /// <param name="neliot">tyhja grid tuodaan mukana</param>
        /// <returns>palauttaa matrixin kokonaisena eli 3x3</returns>
        public List<PhysicsObject> PiirraGrid(List<PhysicsObject> neliot)
        {
            System.Random random = new System.Random();
            int ycount = 1;
            double x = 0;
            double y = 0;
            int gridy = 4;
            int gridx = 3;
            while (ycount < gridy)
            {
                int blueindex = random.Next(0, 3);
                y = ycount * 15;
                int xcount = 0;
                while (xcount < gridx)
                {
                    Color vari = Color.White;
                    if (xcount == blueindex) vari = Color.Blue;
                    if (xcount == 0) x = 0;
                    x = PiirraNelio(this, x, y, vari, neliot);
                    xcount++;
                }
                ycount++;
            }
            return neliot;
        }


        /// <summary>
        /// Aliohjelmalla Piirretaan yksittaitset neliot.
        /// </summary>
        /// <param name="peli">itse peli mihin lisataan</param>
        /// <param name="x">x koordinaatti</param>
        /// <param name="y">y koordinaatti</param>
        /// <param name="vari">kumman varikseksi nelio, sininen/valkoinen</param>
        /// <param name="neliot">matrix tuodaan mukana johon lisataan yksittaiset neliot</param>
        /// <returns>nelion x koordinaatti</returns>
        public double PiirraNelio(PhysicsGame peli, double x, double y, Color vari, List<PhysicsObject> neliot)
        {
            PhysicsObject nelio = new PhysicsObject(150, 150, Shape.Rectangle);
            nelio.Color = vari;
            nelio.X = x;
            nelio.Y = y;
            peli.Add(nelio);
            neliot.Add(nelio);
            x += 30;
            return x;
        }


        /// <summary>
        /// Aliohjelmassa tuhotaan nelio jos se oli sininen. Kaikki neliöt tuhotaan jos saat 3 sinistä tuhottua.
        /// Kun pisteesi ylittävät tietyt lukemat, tuhotaan vanha jouni, ja laitetaan uusi tilalle.
        /// Jos painat valkoisesta, niin pausetaan peli  ja aliohjelma pyytää kirjoittamaan
        ///  nimesi, jos sait tarpeeksi pisteitä päästäksesi top 10 hiscoreissa.
        /// Tämän jälkeen pausee pelin ja odottaa joko F5(pelin uudestaan aloitusta) tai pelin sulkemista.
        /// </summary>
        /// <param name="neliot">yksittainen nelio mika tuhotaan</param>
        /// <param name="pistelaskuri">pistelaskuri tuodaan mukana</param>
        /// <param name="grid">koko matrix tuodaan mukana, jotta sen voi tuhota tarvittaessa</param>
        public void TuhoaNelio(PhysicsObject neliot, IntMeter pistelaskuri, List<PhysicsObject> grid)
        {
            int jounivihainen = 15;
            int jounineutraali = 30;
            int sinistenlkm = 3;
            int neliolkm = 9;
            if (neliot.Color == Color.Blue)
            {
                neliot.Destroy();
                pistelaskuri.Value += 1;
                if (pistelaskuri.Value > jounivihainen)
                {
                    jouni.Destroy();
                    LuoJouni(jouniNeutraali);
                }
                if (pistelaskuri.Value > jounineutraali)
                {
                    jouni.Destroy();
                    LuoJouni(jouniilonen);
                }
                if (pistelaskuri.Value % sinistenlkm == 0)
                {
                    int i = 0;
                    while (i < neliolkm)
                    {
                        grid[i].Destroy();
                        i++;
                    }
                    pelinPyoritys(pistelaskuri);
                    return;
                }
            }
            else
            {
                ClearAll();
                topLista.EnterAndShow(pistelaskuri.Value);
                MessageDisplay.Add("Painoit Valkoisesta! Paina F5 Aloittaaksesi uudelleen. Pisteesi : " + pistelaskuri);
                IsPaused = true;
                Keyboard.Listen(Key.F5, ButtonState.Pressed, AloitaAlusta, "Uusi peli");
                Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
            }
        }


        /// <summary>
        /// Aliohjelmalla Kuunnellaan hiirta etta onko se boxin paalla ja painaa left click.
        /// </summary>
        /// <param name="neliot">koko matrix tuodaan mukana, jonka lapi loopataan</param>
        /// <param name="pistelaskuri">pistelaskuri mukana, etta voi antaa seuraavalle funktiolle</param>
        public void KuunteleHiirta(List<PhysicsObject> neliot, IntMeter pistelaskuri)
        {
            int i = 0;
            int gridinkoko = 9;
            while (i < gridinkoko)
            {
                Mouse.ListenOn(neliot[i], MouseButton.Left, ButtonState.Pressed, TuhoaNelio,
                    null, neliot[i], pistelaskuri, neliot);
                i++;
            }
        }


        /// <summary>
        /// Aliohjelmalla luodaan pistelaskuri.
        /// </summary>
        /// <param name="pisteLaskuri">pistelaskuri mukana, etta sita voi paivittaa</param>
        public void LuoPisteLaskuri(IntMeter pisteLaskuri)
        {
            Label pisteNaytto = new Label();
            pisteNaytto.X = Screen.Left + 500;
            pisteNaytto.Y = Screen.Bottom + 100;
            pisteNaytto.TextColor = Color.Black;
            pisteNaytto.Color = Color.White;
            pisteNaytto.Title = "Pisteet: ";

            pisteNaytto.BindTo(pisteLaskuri);
            Add(pisteNaytto);
        }


        /// <summary>
        /// Aliohjelmalla luodaan timer.
        /// </summary>
        /// <param name="pistelaskuri">pistelaskuri mukaan, etta voi antaa sen aikaloppui funktiolle</param>
        /// <param name="interval">aikalaskurin interval</param>
        /// <param name="yCoord">y koordinaatti, mihin aikalaskuri menee naytolla</param>
        public void LuoAikaLaskuri(IntMeter pistelaskuri, int interval, int yCoord)
        {
     
            Timer aikaLaskuri = new Timer();
            aikaLaskuri.Interval = interval;
            aikaLaskuri.Timeout += delegate { AikaLoppui(pistelaskuri); };
            aikaLaskuri.Start(1);

            Label aikaNaytto = new Label();
            aikaNaytto.TextColor = Color.White;
            aikaNaytto.DecimalPlaces = 1;
            aikaNaytto.Y = Screen.Top - yCoord;
            aikaNaytto.BindTo(aikaLaskuri.SecondCounter);
            Add(aikaNaytto);
        }


        /// <summary>
        /// Tuhoaa kaiken kun aika loppuu, Pyytää inputilla laittamaan nimesi ja tallentaa sen hiscore listaan.
        /// Tämän jälkeen pausee pelin, ja odottaa että pelaaja joko sammuttaa pelin,
        /// tai aloittaa uudestaan painamalla F5.
        /// </summary>
        /// <param name="pistelaskuri">pistelaskuri mukana, jotta voi laittaa sen valuet hiscoreihin</param>
        public void AikaLoppui(IntMeter pistelaskuri)
        {
            ClearAll();
            topLista.EnterAndShow(pistelaskuri.Value);
            MessageDisplay.Add("Aika loppui! Paina F5 aloittaaksesi uudestaan. Pisteesi : "+pistelaskuri);
            IsPaused = true;
            Keyboard.Listen(Key.F5, ButtonState.Pressed, AloitaAlusta, "Uusi peli");
            Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
        }


        /// <summary>
        /// Aliohjelma tuhoaa kaiken, ja sen jalkeen tekee uuden kentan.
        /// </summary>
        public void AloitaAlusta()
        {
            ClearAll();
            Luokentta();
        }
    }
}


