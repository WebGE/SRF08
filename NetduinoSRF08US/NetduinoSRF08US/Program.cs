using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using TelemetreUS;

namespace NetduinoSRF08US
{
    public class Program
    {
        public static void Main()
        {
            // Paramètres du bus I2C
            byte addTelem_I2C = 0x70; // Adresse (7 bits) du télémètre SRF08
            UInt16 Freq = 400; // Fréquence d'horloge du bus I2C en kHz

            // Création d'un objet télémètre SRF08
            SRF08 I2CTelemeter = new SRF08(addTelem_I2C, Freq);

            // Affichage de la version du software du télémètre
            Debug.Print("________________________________________");
            Debug.Print("VerSoft: " + I2CTelemeter.VersSoft);
            // Lecture et affichage de la luminosité 
            Debug.Print("Light: " + I2CTelemeter.LightSensor);
            // Affichage du mode de mesure: Ranging ou ANN
            Debug.Print("Mode: " + I2CTelemeter.Mode);
            Debug.Print("________________________________________");
            Thread.Sleep(2000);


            while (true)
            {
                // Déclenchement, lecture et affichage de la distance en cm                                              
                Debug.Print("Distance: " + I2CTelemeter.ReadRange(SRF08.MeasuringUnits.centimeters_InRangingMode) + "cm");
                // Déclenchement, lecture des registres correspondant au premier echo
                Debug.Print("1st Echo HighByte: " + I2CTelemeter.FirstEchoHighByte + "  " + "1st Echo LowByte: " + I2CTelemeter.FirstEchoLowByte);
                // Déclenchement, lecture et affichage de la distance en inch
                Debug.Print("Distance: " + I2CTelemeter.ReadRange(SRF08.MeasuringUnits.inches_InRangingMode) + "inches");
                // Lecture des registres correspondant au premier echo
                Debug.Print("1st Echo HighByte: " + I2CTelemeter.FirstEchoHighByte + "  " + "1st Echo LowByte: " + I2CTelemeter.FirstEchoLowByte);
                // Déclenchement, lecture et affichage de la distance en microsecondes
                Debug.Print("Distance: " + I2CTelemeter.ReadRange(SRF08.MeasuringUnits.microseconds_InRangingMode) + "µs");
                // Lecture des registres correspondant au premier echo
                Debug.Print("1st Echo HighByte: " + I2CTelemeter.FirstEchoHighByte + "  " + "1st Echo LowByte: " + I2CTelemeter.FirstEchoLowByte);
                // Lecture et affichage de la luminosité 
                Debug.Print("Light: " + I2CTelemeter.LightSensor);
                Debug.Print("________________________________________");
                // Afichage du mode de mesure: Ranging ou ANN
                Debug.Print("Mode: " + I2CTelemeter.Mode);
                Thread.Sleep(1000);
            }
        }

    }
}
