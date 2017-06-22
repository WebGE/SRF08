#define LCD

using Microsoft.SPOT;
using Microtoolskit.Hardware.Displays;
using System.Threading;
using testMicroToolsKit.Hardware.IO;


namespace FezPanda
{
    public class Program
    {
        public static void Main()
        {
#if LCD
            ELCD162 lcd = new ELCD162("COM1");
            lcd.Init(); lcd.ClearScreen(); lcd.CursorOff();
            lcd.PutString("Devantech SRF08");
            lcd.SetCursor(0, 1);
#endif
            SRF08 I2CTelemeter = new SRF08();

            try
            {
                Debug.Print("________________________________________");
                Debug.Print("VerSoft: " + I2CTelemeter.VersSoft);
                Debug.Print("Light: " + I2CTelemeter.LightSensor);
                Debug.Print("Mode: " + I2CTelemeter.Mode);
                Debug.Print("________________________________________");
            }
            catch (System.IO.IOException ex)
            {
#if LCD
                lcd.SetCursor(0, 1); lcd.PutString(ex.Message);
#else
                Debug.Print(ex.Message);
#endif
            }
            finally
            {
                Thread.Sleep(2000);
            }


            while (true)
            {
                try
                {
#if LCD
                    lcd.SetCursor(0, 1);
                    lcd.PutString("                ");
                    lcd.SetCursor(0, 1);
                    lcd.PutString("d = " + I2CTelemeter.ReadRange(SRF08.MeasuringUnits.centimeters_InRangingMode) + "cm");
#else

                    Debug.Print("Distance: " + I2CTelemeter.ReadRange(SRF08.MeasuringUnits.centimeters_InRangingMode) + "cm");
                    Debug.Print("1st Echo HighByte: " + I2CTelemeter.FirstEchoHighByte + "  " + "1st Echo LowByte: " + I2CTelemeter.FirstEchoLowByte);
                    Debug.Print("Distance: " + I2CTelemeter.ReadRange(SRF08.MeasuringUnits.inches_InRangingMode) + "inches");
                    Debug.Print("1st Echo HighByte: " + I2CTelemeter.FirstEchoHighByte + "  " + "1st Echo LowByte: " + I2CTelemeter.FirstEchoLowByte);
                    Debug.Print("Distance: " + I2CTelemeter.ReadRange(SRF08.MeasuringUnits.microseconds_InRangingMode) + "µs");
                    Debug.Print("1st Echo HighByte: " + I2CTelemeter.FirstEchoHighByte + "  " + "1st Echo LowByte: " + I2CTelemeter.FirstEchoLowByte);
                    Debug.Print("Light: " + I2CTelemeter.LightSensor);
                    Debug.Print("________________________________________");
                    Debug.Print("Mode: " + I2CTelemeter.Mode);
#endif
                }
                catch (System.IO.IOException ex)
                {
#if LCD
                    lcd.SetCursor(0, 1);
                    lcd.PutString(ex.Message);
#else
                    Debug.Print(ex.Message);
#endif
                }
                finally
                {
                    Thread.Sleep(5000);
                }
            }
        }
    }
}
