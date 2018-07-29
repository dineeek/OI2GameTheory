﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace OI2GameTheory
{
    public class KalkulatorZakljuckaB
    {
        private DataTable zadnjaTablica;
        private SpremanjeUnosa podaciStrategija;
        private string zakljucak = "";
        private double diferencija;

        public KalkulatorZakljuckaB(DataTable tablica, SpremanjeUnosa podaci, int dif)
        {
            zadnjaTablica = tablica;
            podaciStrategija = podaci;
            diferencija = Convert.ToDouble(dif);

            IzracunajZakljucak();
        }

        double V = 0; //vrijednost igre
        double VCrtano = 0;
        double[] igracAPostoci;
        double[] igracBPostoci;

        private void IzracunajZakljucak()
        {
            VCrtano = (double) 1 / Convert.ToDouble(zadnjaTablica.Rows[zadnjaTablica.Rows.Count - 3][2]);
            V = Math.Round((double) VCrtano - diferencija,2); //V' - D

            //postoci strategija igraca
            igracAPostoci = new double[podaciStrategija.igracA.Count];
            igracBPostoci = new double[podaciStrategija.igracB.Count];

            int brojacB = 0;
            for(int i=2+podaciStrategija.igracA.Count+1; i<zadnjaTablica.Columns.Count-(podaciStrategija.igracB.Count + 2); i++)
            {
                igracBPostoci[brojacB] = Convert.ToDouble(zadnjaTablica.Rows[zadnjaTablica.Rows.Count - 3][i]) * VCrtano;
                brojacB++;
            }

            List<string> sveVarijableA = new List<string>();
            int brojacVarijabliA = 0;
            foreach (var strategija in podaciStrategija.igracA)
            {
                sveVarijableA.Add("x̄" + (brojacVarijabliA + 1));
                brojacVarijabliA++;
            }

            List<string> varijableAUTablici = new List<string>();
            for (int i = 0; i <= zadnjaTablica.Rows.Count - 4; i++)
                varijableAUTablici.Add(zadnjaTablica.Rows[i][1].ToString());


            //provjera podudaranja
            int brojacA = 0;
            foreach(var varijabla in sveVarijableA)
            {
                for (int i=0; i< varijableAUTablici.Count; i++)
                {
                    if (varijabla == (zadnjaTablica.Rows[i][1].ToString()))
                    {
                        igracAPostoci[brojacA] = (double) Convert.ToDouble(zadnjaTablica.Rows[i][2]) * VCrtano;
                        break;
                    }                     
                    else
                        igracAPostoci[brojacA] = 0;
                }
                brojacA++;
            }
     }

        public string DohvatiZakljucak()
        {
            zakljucak = "Vrijednost zadane igre: " + V + Environment.NewLine + "Vjerojatnosti igranja strategija pojedinog igrača: "+Environment.NewLine;
            zakljucak += "Igrač A: ";
            int brojacA = 1;
            foreach (var vjerojatnost in igracAPostoci)
            {
                zakljucak += "X" + brojacA + " = " + Math.Round((vjerojatnost * 100), 2, MidpointRounding.AwayFromZero) + "%   ";
                brojacA++;
            }

            zakljucak += Environment.NewLine;
            zakljucak += "Igrač B: ";

            int brojacB = 1;
            foreach (var vjerojatnost in igracBPostoci)
            {
                zakljucak += "Y" + brojacB + " = " + Math.Round((vjerojatnost * -100), 2, MidpointRounding.AwayFromZero) + "%   ";
                brojacB++;
            }

            return zakljucak;
        }
    }
}