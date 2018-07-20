﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Common;

namespace OI2GameTheory
{
    public class SimplexKalkulator // standardi problem LP za maximum - za igracA se racuna (inače igracB je za min) 
    {
        private SpremanjeUnosa podaciStrategija;
        private int diferencija;

        private DataTable pocetnaSimplexTablica = new DataTable();
        private DataTable prethodnaSimplexTablica = new DataTable(); //pomocna
        private DataTable novaSimplexTablica = new DataTable();

        public DataTable SimplexTablice = new DataTable(); //svi postupci

        public SimplexKalkulator(SpremanjeUnosa podaci, int minDif)
        {
            podaciStrategija = podaci;

            diferencija = Math.Abs(minDif) + 1;
            diferencirajPodatke();

            stvoriPocetnuTablicu();
            pokreniSimplexPostupak();
        }

        private void diferencirajPodatke()
        {
            foreach(var strategija in podaciStrategija.igracA.ToList())//ne mora se i kroz strategije igraca B ići
            {
                for(int i=0; i<strategija.DobitakGubitakStrategije.Length; i++)
                {
                    strategija.DobitakGubitakStrategije[i] += diferencija;
                }
            }
        }

        private void stvoriPocetnuTablicu()
        {
            //stupci
            pocetnaSimplexTablica.Columns.Add("Cj", typeof(int));
            pocetnaSimplexTablica.Columns.Add("Var", typeof(string));
            pocetnaSimplexTablica.Columns.Add("Kol", typeof(double));

            for(int i=0; i<podaciStrategija.igracB.Count; i++)
                pocetnaSimplexTablica.Columns.Add("Ῡ" + (i + 1) + "", typeof(double)); //Ῡ - supstitucija za yi/v'

            for (int i = 0; i < podaciStrategija.igracA.Count; i++)
                pocetnaSimplexTablica.Columns.Add("u" + (i + 1) + "", typeof(double)); //dopunske varijable - ovise o broju jednadzbi tj. igracu 

            pocetnaSimplexTablica.Columns.Add("Kontrola", typeof(double));
            pocetnaSimplexTablica.Columns.Add("Rezultat", typeof(double));

            //redci
            int brojacStrategijaA = 0;
            foreach(var strategijaA in podaciStrategija.igracA)
            {
                double internKontrol = 0;
                var noviRedak = pocetnaSimplexTablica.NewRow();

                noviRedak["Cj"] = 0;
                noviRedak["Var"] = "u" + (brojacStrategijaA + 1) + "";
                noviRedak["Kol"] = 1; //sve strategije (x1+x2.. = 1)
                internKontrol += 1;

                for (int j = 0; j < strategijaA.DobitakGubitakStrategije.Length; j++)
                {
                    noviRedak["Ῡ" + (j + 1) + ""] = strategijaA.DobitakGubitakStrategije[j];
                    internKontrol += strategijaA.DobitakGubitakStrategije[j];
                }

                for (int j = 0; j < podaciStrategija.igracA.Count; j++)
                {

                    if ((j + 1) == (brojacStrategijaA + 1))
                    {
                        noviRedak["u" + (j + 1) + ""] = 1;
                        internKontrol += 1;
                    }
                    else
                        noviRedak["u" + (j + 1) + ""] = 0;
                }

                noviRedak["Kontrola"] = internKontrol;
                pocetnaSimplexTablica.Rows.Add(noviRedak);
                brojacStrategijaA++;
            }

            //Zj-Cj redak
            var redakZjCj = pocetnaSimplexTablica.NewRow();
            redakZjCj["Var"] = "Zj-Cj";
            redakZjCj["Kol"] = 0;
            double kontrolaRedka = 0;//kolicina

            foreach(var strategija in podaciStrategija.igracA)
            {
                for (int i = 0; i < strategija.DobitakGubitakStrategije.Length; i++)
                {
                    redakZjCj["Ῡ" + (i + 1) + ""] = -1;
                }

                for(int i = 0; i < podaciStrategija.igracA.Count; i++)
                {
                    redakZjCj["u" + (i + 1) + ""] = 0;
                }
                kontrolaRedka += -1;
            }

            redakZjCj["Kontrola"] = kontrolaRedka;
            pocetnaSimplexTablica.Rows.Add(redakZjCj);

            //prazan redak radi preglednosti
            var prazanRedak1 = pocetnaSimplexTablica.NewRow();
            pocetnaSimplexTablica.Rows.Add(prazanRedak1);

            SimplexTablice.Merge(pocetnaSimplexTablica);

            prethodnaSimplexTablica = pocetnaSimplexTablica;
        }

        private (int, string) odrediVodeciStupac()
        {
            int indexStupca = 0;
            string nazivSupca = "";

            double najveci = 0;
            double internHelp = 0;

            //koliko ima neg. u zadnjem redku
            int brojNegativnih = 0;
            for (int i = 3; i < prethodnaSimplexTablica.Columns.Count - 2; i++)
            {
                internHelp = Convert.ToDouble(prethodnaSimplexTablica.Rows[prethodnaSimplexTablica.Rows.Count - 2][i].ToString());
                if (internHelp < 0)
                {
                    brojNegativnih++;
                }
            }

            double[] vrijednostiRedaZjCj = new double[brojNegativnih];
            int[] indexiVrijSupca = new int[brojNegativnih];
            int brojac = 0;
            for (int i=3; i<prethodnaSimplexTablica.Columns.Count-2; i++)
            {
                internHelp = Convert.ToDouble(prethodnaSimplexTablica.Rows[prethodnaSimplexTablica.Rows.Count - 2][i].ToString());
                if (internHelp < 0)
                {
                    internHelp = Math.Abs(internHelp);
                    vrijednostiRedaZjCj[brojac] = internHelp;
                    indexiVrijSupca[brojac] = i;
                    brojac++;

                    if (najveci < internHelp)
                    {
                        najveci = internHelp;
                        indexStupca = i;
                        nazivSupca = prethodnaSimplexTablica.Columns[i].ColumnName;
                    }
                }
            }

            double prviBroj = vrijednostiRedaZjCj[0];
            bool sviIsti = false;
            sviIsti = vrijednostiRedaZjCj.Skip(1)
              .All(s => double.Equals(prviBroj, s));

            if (vrijednostiRedaZjCj.Count() == 1)
            {
                return (indexStupca, nazivSupca);
            }
            else if (sviIsti) 
            {
                double najvecaVrijednostStupca = 0;
                for(int i = 0; i < indexiVrijSupca.Length; i++)
                {
                    for (int j = 0; j < podaciStrategija.igracA.Count; j++)
                    {
                        double vrijednostStupca = Convert.ToDouble(prethodnaSimplexTablica.Rows[j][indexiVrijSupca[i]].ToString());
                        if (najvecaVrijednostStupca < vrijednostStupca)
                        {
                            najvecaVrijednostStupca = vrijednostStupca;
                            indexStupca = indexiVrijSupca[i];
                            nazivSupca = prethodnaSimplexTablica.Columns[i+3].ColumnName;
                        }
                    }
                }
                return (indexStupca, nazivSupca);
            }
            else
            {
                return (indexStupca, nazivSupca);
            }
        }

        private int odrediVodeciRedak(int indexStupca) //kaj ak imam iste vrijednosti rezultata
        {
            int indexReda = 0;

            double internHelp = 0;
            double[] rezultati = new double[prethodnaSimplexTablica.Rows.Count - 2];

            for (int i = 0; i<prethodnaSimplexTablica.Rows.Count-2; i++)
            {
                internHelp = Convert.ToDouble(prethodnaSimplexTablica.Rows[i][2]) / Convert.ToDouble(prethodnaSimplexTablica.Rows[i][indexStupca]);              
                rezultati[i] = internHelp;
                if(internHelp > 0)
                    prethodnaSimplexTablica.Rows[i][prethodnaSimplexTablica.Columns.Count - 1] = Math.Round(internHelp, 4);
            }
            SimplexTablice.Merge(prethodnaSimplexTablica);
            double najmanji = rezultati.Where(x => x > 0).Min();
            
            for (int i= 0; i<rezultati.Length; i++)
            {
                if(najmanji == rezultati[i])
                {
                    //najmanji = rezultati[i];
                    indexReda = i;
                }
            }           
            return indexReda;
        }

        private double odrediStozerniElement(int indexStupca, int indexReda)
        {
            double stozerniElement = Convert.ToDouble(prethodnaSimplexTablica.Rows[indexReda][indexStupca]);
            //SimplexTablice.Merge(prethodnaSimplexTablica);
            return stozerniElement;
        }

        private void izracunajElementeVodecegRedka(int indexStupca, int indexRedka, double stozerniElement, string nazivVodSupca)
        {
            for(int i=2; i<prethodnaSimplexTablica.Columns.Count-1; i++)
            {
                double internHelp = Convert.ToDouble(prethodnaSimplexTablica.Rows[indexRedka][i]);
                novaSimplexTablica.Rows[indexRedka][i] = (double) internHelp / stozerniElement;
            }

            novaSimplexTablica.Rows[indexRedka][0] = 1; //vrijednost Cj = 1
            novaSimplexTablica.Rows[indexRedka][1] = nazivVodSupca; //vrijednost var

            for(int i=0; i<prethodnaSimplexTablica.Rows.Count-1; i++)
            {
                if (i != indexRedka)
                    novaSimplexTablica.Rows[i][indexStupca] = 0;
            }
        }

        private void simplexAlgoritam(int indexStupca, int indexRedka, double stozerniElement)
        {
            //od index 2 do count-2
            for (int i = 2; i < novaSimplexTablica.Columns.Count - 1; i++)//stupci
            {
                if(i != indexStupca)
                {
                    for (int j = 0; j < novaSimplexTablica.Rows.Count - 1; j++)//redci
                    {
                        if(j != indexRedka)
                        {
                            double internHelp = Convert.ToDouble(prethodnaSimplexTablica.Rows[j][i].ToString()) - ((double)((double)(Convert.ToDouble(prethodnaSimplexTablica.Rows[indexRedka][i].ToString()) / stozerniElement) * Convert.ToDouble(prethodnaSimplexTablica.Rows[j][indexStupca].ToString())));
                            novaSimplexTablica.Rows[j][i] = Math.Round(internHelp, 4);
                        }
                    }
                }
            }
        }
        private void pokreniSimplexPostupak()
        {
            int indexStupca = odrediVodeciStupac().Item1;
            string nazivVodecegStupca = odrediVodeciStupac().Item2;

            int indexRedka = odrediVodeciRedak(indexStupca);

            double stozerniElement = odrediStozerniElement(indexStupca, indexRedka);

            novaSimplexTablica = prethodnaSimplexTablica.Clone();//da naslijedi strukturu samo

            for(int i=0; i<prethodnaSimplexTablica.Rows.Count; i++)//dodavanje redaka u novu
            {
                var noviRedak = novaSimplexTablica.NewRow();
                novaSimplexTablica.Rows.Add(noviRedak);
            }

            for (int i = 0; i < prethodnaSimplexTablica.Rows.Count-1; i++)//prepisivanje statike
            {
                if(i != indexRedka)
                {
                    novaSimplexTablica.Rows[i][0] = prethodnaSimplexTablica.Rows[i][0];//Cj stupac
                    novaSimplexTablica.Rows[i][1] = prethodnaSimplexTablica.Rows[i][1];//Var stupac
                }
            }

            izracunajElementeVodecegRedka(indexStupca, indexRedka, stozerniElement, nazivVodecegStupca);

            simplexAlgoritam(indexStupca, indexRedka, stozerniElement);

            SimplexTablice.Merge(novaSimplexTablica); //prije if-a obavezno

            //koliko ima neg. u zadnjem redku
            int brojNegativnih = 0;
            for (int i = 3; i < novaSimplexTablica.Columns.Count - 2; i++)
            {
                double internHelp = Convert.ToDouble(novaSimplexTablica.Rows[novaSimplexTablica.Rows.Count - 2][i].ToString());
                if (internHelp < 0)
                {
                    brojNegativnih++;
                }
            }

            if(brojNegativnih != 0)
            {
                //prethodnaSimplexTablica = novaSimplexTablica;
                prethodnaSimplexTablica = new DataTable();
                prethodnaSimplexTablica = novaSimplexTablica.Copy();//da naslijedi strukturu samo
                novaSimplexTablica = new DataTable();

                pokreniSimplexPostupak();
            }
            else
            {
                //pisanje iteracija tako da je pregledno i jasnije
                int interHelp = SimplexTablice.Rows.Count-1;
                int brojRedovaIteracije = (novaSimplexTablica.Rows.Count*2)-1;
                int brojRedova = (novaSimplexTablica.Rows.Count * 2);
                int brojIteracija = 1;
                for (int i=0; i< interHelp; i++)
                {
                    if (i == brojRedovaIteracije)
                    {
                        SimplexTablice.Rows[i][1] ="Tablica "+ brojIteracija+". iteracije";
                        brojRedovaIteracije += brojRedova;
                        brojIteracija++;
                    }

                }
                System.Windows.Forms.MessageBox.Show("KRAJ! TREBA ZAKLJUČAK IZRAČUNATI!");
            }
            //if(novaSimplexTablica treba daljnji postupak) AKO SU SVE VRIJEDNOSTI U ZjCj redku >= 0
            //prethodnaSimplexTablica = novaSimplexTablica;
            //novaSimplex.Clear();
            //pokreniSimplexPostupak()
            //else
            //gotovo
        }
    }
}
