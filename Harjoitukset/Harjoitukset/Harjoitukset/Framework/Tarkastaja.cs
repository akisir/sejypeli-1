using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Effects;
using Jypeli.Widgets;
using System.Linq;

public abstract class Tarkistaja : PhysicsGame
{
    enum TehtavanTila
    {
        EiToteutettu,
        ToteutettuSaattaaToimia,
        ToteutettuEiToimi,
        ToteutettuToimii,
    }

    int PACPADDING = 60;
    int PACSPEED = 333;
    int TEHTAVIA = 10;
    int tehtava = 0;
    GameObject pacman;
    GameObject haamut;
    List<GameObject> kolikot;
    int objektienLukumaaraTehtava4 = 0;

    // Tehtävien tilatiedot
    List<GameObject> objektitEnnenTehtavaa = null;
    List<GameObject> talletetutObjektitTehtava3 = null;
    List<GameObject> talletetutObjektitTehtava4 = null;

    public override void Begin()
    {
        SetWindowSize(1280, 720);

        Animation pacanim = new Animation( LoadImages("pac1","pac2") );
        pacman = new GameObject(pacanim);
        pacman.X = Screen.Left + PACPADDING;
        pacman.Y = Screen.Bottom + PACPADDING;
        Add(pacman, 2);
        pacman.Animation.FPS = 3;
        pacman.Animation.Start();

        Animation haamuanim = new Animation(LoadImages("haamu1", "haamu2", "haamu3"));
        haamut = new GameObject(haamuanim);
        haamut.X = Screen.Right + PACPADDING + haamut.Width / 2;
        haamut.Y = Screen.Bottom + PACPADDING;
        Add(haamut, 2);
        haamut.Animation.FPS = 3;
        haamut.Animation.Start();

        kolikot = new List<GameObject>();
        double kolikkoy = pacman.Y;
        double kolikkox = pacman.X;
        for (int i = 0; i < TEHTAVIA; i++)
        {
            kolikkox += (Screen.Right - PACPADDING - pacman.X) / TEHTAVIA;

            GameObject kolikko = new GameObject(40,40);
            kolikko.Image = LoadImage("coin");
            kolikko.X = kolikkox;
            kolikko.Y = kolikkoy;
            kolikot.Add(kolikko);
            Add(kolikko, 1);
        }
        
        Timer.SingleShot(0.33, TarkistaTehtava);

        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
    }

    private double PoistaKolikko()
    {
        double seuraavaanTarkastukseen = 1.00;
        if (tehtava > 0)
        {
            if (tehtava > 1)
                kolikot[tehtava - 2].Destroy();
            Vector coinPos = kolikot[tehtava - 1].Position;
            coinPos.X += 20;
            pacman.MoveTo(coinPos, PACSPEED);

            // Prevent removing coin before check
            seuraavaanTarkastukseen = Math.Abs(pacman.X - coinPos.X) / PACSPEED;
        }
        return seuraavaanTarkastukseen;
    }

    private void TarkistaTehtava()
    {
        TehtavanTila tulos = TehtavanTila.EiToteutettu;

        switch (tehtava)
        {
            case 1:
                tulos = TarkistaTehtava1();
                break;
            case 2:
                tulos = TarkistaTehtava2();
                break;
            case 3:
                tulos = TarkistaTehtava3();
                break;
            case 4:
                tulos = TarkistaTehtava4();
                break;
            case 5:
                tulos = TarkistaTehtava5();
                break;
            case 6:
                //TODO: Aki/Jussi kirjoita tarkistakoodi
                break;
            case 7:
                //TODO: Aki/Jussi kirjoita tarkistakoodi
                break;
            case 8:
                //TODO: Aki/Jussi kirjoita tarkistakoodi
                break;
            case 9:
                //TODO: Aki/Jussi kirjoita tarkistakoodi
                break;
            case 10:
                //TODO: Aki/Jussi kirjoita tarkistakoodi
                break;
            default:
                tulos = TehtavanTila.ToteutettuToimii;
                break;
        }

        // Toimi tuloksen mukaisesti
        if (tulos == TehtavanTila.ToteutettuToimii)
        {
            double seuraavaanTarkastukseen = PoistaKolikko();
            tehtava++;

            // Try as long as it takes.
            Timer.SingleShot(seuraavaanTarkastukseen, TarkistaTehtava);
        }
        else if (tulos == TehtavanTila.ToteutettuEiToimi)
        {
            PoistaKolikko();
            Vector ulosVasemmalta = new Vector(Screen.Left-haamut.Width/2, pacman.Y);
            Animation peruutusAnimaatio = Animation.Mirror(new Animation(LoadImages("pac1", "pac2")));
            pacman.Animation = peruutusAnimaatio;
            pacman.Animation.FPS = 5;
            pacman.Animation.Start();
            pacman.MoveTo(ulosVasemmalta, PACSPEED);
            haamut.MoveTo(ulosVasemmalta, PACSPEED);
        }
        else if (tulos == TehtavanTila.ToteutettuSaattaaToimia)
        {
            Timer.SingleShot(0.01, TarkistaTehtava);
        }
        
    }

    private TehtavanTila TarkistaTehtava5()
    {
        TehtavanTila tila = TehtavanTila.EiToteutettu;
        try
        {
            if (objektitEnnenTehtavaa == null)
            {
                // Precondition
                objektitEnnenTehtavaa = GetObjects(go => go is PhysicsObject);
                Tehtava5();
                // Tehtavan kutsumisen jälkeen pitää antaa JyPelille aikaa tehdä työnsä.
                //  tähän tarkistajaan tullaan siis myöhemmin uudelleen, mutta tätä haaraa ei enää tehdä.
                tila = TehtavanTila.ToteutettuSaattaaToimia;
                return tila;
            }

            List<GameObject> uudet = GetObjects(go => go is PhysicsObject && !objektitEnnenTehtavaa.Contains(go));

            /*string teksti = String.Format("left {0}, position {1}, top {2}, right {3}, X {4}, bottom {5}, Y {6}", 
                Math.Round(uudet[0].Left), uudet[0].Position, (int)uudet[0].Top, (int)uudet[0].Right, uudet[0].X, (int)uudet[0].Bottom, uudet[0].Y);
            MessageDisplay.Add(teksti);
            string leveltext = String.Format(" bottom, {0}, height {1}, left {2}, size {3}, top {4}, width {5} ", 
                Level.Bottom, Level.Height, Level.Left, Level.Size, Level.Top, Level.Width);
            MessageDisplay.Add(leveltext);*/

            tila = TehtavanTila.ToteutettuEiToimi;
            if (uudet.Count < 4)
            {
                MessageDisplay.Add("Et ole lisännyt kaikkia neljää reunaa");
            }
            else if (uudet.Count > 4)
            {
                MessageDisplay.Add("Olet lisännyt liian monta pelioliota");
            }
            else if (!TarkistaOlioidenMuoto(Shape.Rectangle, uudet))
            {
                MessageDisplay.Add("Reunat ovat väärän muotoiset");
            }
            else if (!TarkistaReunat(uudet))
            {
                MessageDisplay.Add("Reunat ovat väärän kokoiset tai väärässä paikassa");
            }
            else
            {
                tila = TehtavanTila.ToteutettuToimii;
                objektitEnnenTehtavaa.Clear();
                objektitEnnenTehtavaa = null;
            }
        }
        catch (NotImplementedException)
        {
            tila = TehtavanTila.EiToteutettu;
        }
        return tila;
    }

    private TehtavanTila TarkistaTehtava4()
    {
        TehtavanTila tila = TehtavanTila.EiToteutettu;
        try
        {
            if (objektitEnnenTehtavaa == null)
            {
                objektienLukumaaraTehtava4 = RandomGen.NextInt(100, 150);
                // Precondition
                objektitEnnenTehtavaa = GetObjects(go => go is PhysicsObject);
                Tehtava4(objektienLukumaaraTehtava4);
                // Tehtavan kutsumisen jälkeen pitää antaa JyPelille aikaa tehdä työnsä.
                //  tähän tarkistajaan tullaan siis myöhemmin uudelleen, mutta tätä haaraa ei enää tehdä.
                tila = TehtavanTila.ToteutettuSaattaaToimia;
                return tila;
            }

            List<GameObject> talletetutObjektitTehtava4 = GetObjects(go => go is PhysicsObject && !objektitEnnenTehtavaa.Contains(go));

            tila = TehtavanTila.ToteutettuEiToimi;
            if (talletetutObjektitTehtava4.Count < objektienLukumaaraTehtava4)
            {
                MessageDisplay.Add("Peliolioita ei ole lisätty tarpeeksi monta");
            }
            else if (talletetutObjektitTehtava4.Count > objektienLukumaaraTehtava4)
            {
                MessageDisplay.Add("Liian monta uutta pelioliota lisätty");
            }
            else if (!TarkistaOlioidenMuoto(Shape.Circle, talletetutObjektitTehtava4))
            {
                MessageDisplay.Add("Lisäämäsi pallo ei ole pyöreä");
            }
            else if (!TarkistaOlioidenVari(Color.White, talletetutObjektitTehtava4))
            {
                MessageDisplay.Add("Pallot eivät ole valkoisia!");
            }
            else if (talletetutObjektitTehtava4[0].Position == new Vector(0, 0))
            {
                MessageDisplay.Add("Pallosi on keskellä ruutua. Oletko varma, että laitoit sen satunnaiseen paikkaan? Yritä uudestaan");
            }
            else if (!TarkistaOlioidenEtaisyys(300, talletetutObjektitTehtava4))
            {
                MessageDisplay.Add("Pallosi on liian kaukana keskipisteestä. Yritäppä uudestaan varmuuden vuoksi");
            }
            else
            {
                tila = TehtavanTila.ToteutettuToimii;
                objektitEnnenTehtavaa.Clear();
                objektitEnnenTehtavaa = null;

                //Timer.SingleShot(0.1, TuhoaPallot);
            }
        }
        catch (NotImplementedException)
        {
            tila = TehtavanTila.EiToteutettu;
        }
        return tila;
    }

    private TehtavanTila TarkistaTehtava3()
    {
        TehtavanTila tila = TehtavanTila.EiToteutettu;
        try
        {
            if (objektitEnnenTehtavaa == null)
            {
                // Precondition
                objektitEnnenTehtavaa = GetObjects(go => go is PhysicsObject);
                Tehtava3();
                // Tehtavan kutsumisen jälkeen pitää antaa JyPelille aikaa tehdä työnsä.
                //  tähän tarkistajaan tullaan siis myöhemmin uudelleen, mutta tätä haaraa ei enää tehdä.
                tila = TehtavanTila.ToteutettuSaattaaToimia;
                return tila;
            }

            List<GameObject> uudet = GetObjects(go => go is PhysicsObject && !objektitEnnenTehtavaa.Contains(go));
            talletetutObjektitTehtava3 = uudet;

            tila = TehtavanTila.ToteutettuEiToimi;
            if (uudet.Count == 0)
            {
                MessageDisplay.Add("Punaista palloa edustavaa pelioliota ei ole lisätty");
            }
            else if (uudet.Count > 1)
            {
                MessageDisplay.Add("Liian monta uutta pelioliota lisätty");
            }
            else if (uudet[0].Shape != Shape.Circle)
            {
                MessageDisplay.Add("Lisäämäsi pallo ei ole pyöreä");
            }
            else if (uudet[0].Color != Color.Red)
            {
                MessageDisplay.Add("Pallosi ei ole punainen, muistitko vaihtaa värin oikein!");
            }
            else
            {
                tila = TehtavanTila.ToteutettuToimii;
                objektitEnnenTehtavaa.Clear();
                objektitEnnenTehtavaa = null;
            }
        }
        catch (NotImplementedException)
        {
            tila = TehtavanTila.EiToteutettu;
        }
        return tila;
    }

    private TehtavanTila TarkistaTehtava2()
    {
        TehtavanTila tila = TehtavanTila.EiToteutettu;
        try
        {
            tila = TehtavanTila.ToteutettuToimii;

            double TOLERANCE = 0.01;
            for (int i = 0; i < 100; i++)
            {
                double x = RandomGen.NextDouble(-10.0, 10.0);
                double y = RandomGen.NextDouble(-10.0, 10.0);
                double z = RandomGen.NextDouble(-10.0, 10.0);
                double ka = (x + y + z) / 3;
                if (Math.Abs(ka - Tehtava2(x, y, z)) > TOLERANCE)
                {
                    string virhe = String.Format(@"Virhe: Koodi ei laske oikein keskiarvoa. Esim. koodi antaa
{0}, {1}, {2} keskiarvoksi {3} kun sen pitäisi olla {4}", x, y, z, Tehtava2(x, y, z), (x + y + z / 3));
                    MessageDisplay.Add(virhe);

                    tila = TehtavanTila.ToteutettuEiToimi;
                    break;
                }
            }
        }
        catch (NotImplementedException)
        {
            tila = TehtavanTila.EiToteutettu;
        }
        return tila;
    }

    private TehtavanTila TarkistaTehtava1()
    {
        TehtavanTila tila = TehtavanTila.EiToteutettu;
        try
        {
            tila = TehtavanTila.ToteutettuToimii;

            for (int i = 0; i < 100; i++)
            {
                int a = RandomGen.NextInt(10);
                int b = RandomGen.NextInt(10);
                if (a + b != Tehtava1(a, b))
                {
                    string virhe = String.Format("Virhe: Koodi ei laske oikein lukuja yhteen. Esim. {0}+{1}={2}, kun pitäisi olla {3}", a, b, Tehtava1(a, b), a + b);
                    MessageDisplay.Add(virhe);
                    tila = TehtavanTila.ToteutettuEiToimi;
                    break;
                }   
            }
        }
        catch (NotImplementedException)
        {
            tila = TehtavanTila.EiToteutettu;
        }
        return tila;
    }

    bool TarkistaOlioidenMuoto(Shape muoto, List<GameObject> oliot )
    {
        //MessageDisplay.Add("muoto");
        for (int i = 0; i < oliot.Count; i++)
        {
            if (oliot[i].Shape != muoto)
            {
                //MessageDisplay.Add("muoto false");
                return false;
            }
        }

        return true;
    }

    bool TarkistaOlioidenVari(Color vari, List<GameObject> oliot)
    {
        for (int i = 0; i < oliot.Count; i++)
        {
            if (oliot[i].Color != vari)
            {
                return false;
            }
        }

        return true;
    }

    bool TarkistaOlioidenEtaisyys(int maxEtaisyys, List<GameObject> oliot)
    {
        for (int i = 0; i < oliot.Count; i++)
        {
            if (oliot[i].X > maxEtaisyys || oliot[i].Y > maxEtaisyys)
            {
                return false;
            }
        }

        return true;
    }

    bool TarkistaReunat(List<GameObject> oliot)
    {
        bool returnValue = true;
        
        for (int i = 0; i < oliot.Count; i++ )
        {
            // Tarkista on vasen tai oikea reuna
            if (oliot[i].Y == 0)
            {
                // Tarkista koko
                if (oliot[i].Width != Level.Height)
                {
                    returnValue = false;
                }
                // Tarista paikka
                else if ((Math.Abs(oliot[i].X) - (oliot[i].Height/2)) != (Level.Width/2))
                {
                    returnValue = false;
                }
            }
            // Tarkista onko ylä tai ala reuna
            else if (oliot[i].X == 0)
            {
                // Tarkista koko
                if (oliot[i].Width != (Level.Width+(oliot[i].Height*2)))
                {
                    returnValue = false;
                }
                // Tarkista paikka
                else if ((Math.Abs(oliot[i].Y) - (oliot[i].Height/2)) != (Level.Height / 2))
                {
                    returnValue = false;
                }
            }
        }

        return returnValue;
    }

    /*
     * Tehtävänanto: Palauta muuttujien a ja b summa (plus lasku)
     */
    public virtual int Tehtava1(int a, int b) { throw new NotImplementedException(); }

    /*
     * Tehtävänanto: Palauta muuttujien z, y ja z keskiarvo
     * http://tilastokoulu.stat.fi/verkkokoulu_v2.xql?page_type=sisalto&course_id=tkoulu_tlkt&lesson_id=4&subject_id=4
     */
    public virtual double Tehtava2(double x, double y, double z) { throw new NotImplementedException(); }

    /*
     * Tehtävänanto: Lisää PUNAINEN pallo peliin
     */
    public virtual void Tehtava3() { throw new NotImplementedException(); }

    /*
     * Tehtävänanto: Lisää peliin n kpl satunnaisessa paikassa olevaa VALKOISTA palloa
     * korkeintaan 300.0 päähän ruudun keskipisteestä
     * Vector paikka = RandomGen.NextVector(0.0, 300.0);
     * (vinkki, käytä for-silmukkaa)
     */
    public virtual void Tehtava4(int n) { throw new NotImplementedException(); }

    /*
     * Tehtävänanto: Lisää peliin reunat joka puolelle
     */
    public virtual void Tehtava5() { throw new NotImplementedException(); }

    /*
     * Tehtävänanto: Lyö PUNAISELLE pallolle vauhtia satunnaiseen suuntaan
     *  (vinkki, tee Tehtava4-aliohjelmassa lisättävästä pallosta luokkamuuttuja) 
     */
    public virtual void Tehtava6() { throw new NotImplementedException(); }

    /*
     * Tehtävänanto: Kun kaksi VALKOISTA palloa osuu toisiinsa, pistä ne katoamaan
     *  lisäpisteitä jos saat ne räjähtämään (kersku siitä kaverille ja opettajille :)
     *  (vinkki, tarvitset törmäyskäsittelijää ja uuden aliohjelman)
     */
    public virtual void Tehtava7() { throw new NotImplementedException(); }

    /*
     * Tehtävänanto: Aina kun välilyöntiä painetaan, lyö PUNAISELLE pallolle lisää vauhtia.
     * (vinkki: uudelleenkäytä Tehtava7-aliohjelmaa kutsumalla sitä näin "Tehtava7();")
     */
    public virtual void Tehtava8() { throw new NotImplementedException(); }

    /*
     * Tehtävänanto: ??
     * (vinkki: ??)
     */
    public virtual void Tehtava9() { throw new NotImplementedException(); }

    /*
     * Tehtävänanto: Lisää ruudulle laskuri, joka pitää kirjaa siitä montako VALKOISTA
     *  palloa on vielä pelissä. Kun kaikki valkoiset pallot ovat kadonneet, lisää uusi
     *  satsi palloja käyttäen Tehtava5()-aliohjelmaa.
     */
    public virtual void Tehtava10() { throw new NotImplementedException(); }
}
