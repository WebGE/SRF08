using System;
using System.Threading;
using Microsoft.SPOT.Hardware;


namespace ToolBoxes
{
    public class SRF08
        {
            // I2C
            private const Int16 TRANSACTIONEXECUTETIMEOUT = 1000;
            private I2CDevice busI2C = null;
            private I2CDevice.Configuration ConfigSRF08 = null;

            // Field
            private MeasuringUnits unit = MeasuringUnits.undefined;

            // Enumerations
            /// <summary>
            /// SRF08 Registers list
            /// </summary>
            private enum Registers : byte
            {
                SoftRev=0,
                Command=0,
                LightSensor,
                _1stEchoH,
                _1stEchoL
            }
            /// <summary>
            /// SRF08 ranging Mode and Unity
            /// </summary>
            public enum MeasuringUnits : byte
            {
                inches_InRangingMode,
                centimeters_InRangingMode,
                microseconds_InRangingMode,
                inches_InANNMode,
                centimeters_InANNMode,
                microseconds_InANNMode,
                undefined
            };

            // Constructors
            /// <summary>
            /// Constructor with Slave Address = 0x70 and Bus Frequency = 100kHz
            /// </summary>
            public SRF08()
            {
                ConfigSRF08 = new I2CDevice.Configuration(0x70, 100);
            }
            /// <summary>
            /// Constructor with Bus Frequency = 100kHz
            /// </summary>
            /// <param name="I2C_Add_7bits">ADDR in 0x70 to 0x7F</param>
            public SRF08(byte I2C_Add_7bits)
            {
                ConfigSRF08 = new I2CDevice.Configuration(I2C_Add_7bits, 100);
            }
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="I2C_Add_7bits">ADDR in 0x70 to 0x7F</param>
            /// <param name="FreqBusI2C">400kHz max</param>
            public SRF08(ushort I2C_Add_7bits, UInt16 FreqBusI2C)
            {
                ConfigSRF08 = new I2CDevice.Configuration(I2C_Add_7bits, FreqBusI2C);               
            }

            // Properties  
            /// <summary>
            /// Software Revision Register Get Access
            /// </summary>
            public byte VersSoft
            {
                get
                {
                    byte verSoft = GetRegister(Registers.SoftRev);
                    return verSoft;
                }
            }

            /// <summary>
            /// Mode Get Access
            /// </summary>
            public string Mode
            {
                get
                {
                    string mode;
                    switch (unit)
                    {
                        case MeasuringUnits.centimeters_InRangingMode:
                        case MeasuringUnits.inches_InRangingMode:
                        case MeasuringUnits.microseconds_InRangingMode: mode = "Ranging"; break;
                        case MeasuringUnits.centimeters_InANNMode:
                        case MeasuringUnits.inches_InANNMode:
                        case MeasuringUnits.microseconds_InANNMode: mode = "ANN"; break;
                        default: mode = "undefined"; break;
                    }
                    return mode;
                }
            }

            /// <summary>
            /// Light Sensor Register Get Access
            /// </summary>
            /// <return></return>
            public byte LightSensor
            {
                get
                {
                    byte lightSensor = GetRegister(Registers.LightSensor);
                    return lightSensor;
                }
            }            

            /// <summary>
            /// 1st Echo High Byte
            /// </summary>
            public byte FirstEchoHighByte
            {
                get
                {
                    byte firstEchoHighByte = GetRegister(Registers._1stEchoH);
                    return firstEchoHighByte;
                }
            }

            /// <summary>
            /// 1st Echo Low Byte Get Access
            /// </summary>
            public byte FirstEchoLowByte
            {
                get
                {
                    byte firstEchoLowByte = GetRegister(Registers._1stEchoL);
                    return firstEchoLowByte;
                }
            }

            /// <summary>
            /// Triggers a shot ulrasons, wait for 75ms and return result in the unit of measure
            /// </summary>
            /// <param name="units">unit of measure expected</param>
            /// <returns>range in cm or inches or millisec</returns>
            public UInt16 ReadRange(MeasuringUnits units)
            {
                this.unit = units;
                // Calcul du mot de commande à partir de l'unité de mesure
                byte command = (byte)(80 + (byte)units);

                // Création d'un buffer et d'une transaction pour l'accès au module en écriture
                byte[] outbuffer = new byte[] { (byte)Registers.Command, command };
                I2CDevice.I2CTransaction WriteUnit = I2CDevice.CreateWriteTransaction(outbuffer);

                // Création d'un buffer et d'une transaction pour l'accès au module en lecture
                byte[] inbuffer = new byte[4];
                I2CDevice.I2CTransaction ReadDist = I2CDevice.CreateReadTransaction(inbuffer);

                // Tableaux des transactions 
                I2CDevice.I2CTransaction[] T_WriteUnit = new I2CDevice.I2CTransaction[] { WriteUnit };
                I2CDevice.I2CTransaction[] T_ReadDist = new I2CDevice.I2CTransaction[] { ReadDist };
                
                // Exécution des transactions
                busI2C = new I2CDevice(ConfigSRF08); // Connexion virtuelle de l'objet SRF08 au bus I2C 
                busI2C.Execute(T_WriteUnit, TRANSACTIONEXECUTETIMEOUT); // Transaction : Activation US
                Thread.Sleep(75); // attente echo US
                busI2C.Execute(T_ReadDist, TRANSACTIONEXECUTETIMEOUT); // Transaction : Lecture distance
                
                UInt16 range = (UInt16)((UInt16)(inbuffer[3] << 8) + inbuffer[2]); // Calcul de la distance
                busI2C.Dispose(); // Déconnexion virtuelle de l'objet SRF08 du bus I2C
                return range; 
            }
            /// <summary>
            /// Only triggers a shot ulrasons
            /// </summary>
            /// <param name="units">unit of measure expected</param>
            public void TrigShotUS(MeasuringUnits units)
            {
                this.unit = units;
                // Calcul du mot de commande à partir de l'unité de mesure
                byte commandByte = (byte)(80 + (byte)units);

                // Création d'un buffer et d'une transaction pour l'accès au module en écriture
                byte[] outbuffer = new byte[] { (byte)Registers.Command, commandByte };
                I2CDevice.I2CTransaction WriteUnit = I2CDevice.CreateWriteTransaction(outbuffer);

                // Tableaux des transactions 
                I2CDevice.I2CTransaction[] T_WriteUnit = new I2CDevice.I2CTransaction[] { WriteUnit };

                // Exécution de la transactions
                busI2C = new I2CDevice(ConfigSRF08); // Connexion virtuelle de l'objet SRF08 au bus I2C 
                busI2C.Execute(T_WriteUnit, TRANSACTIONEXECUTETIMEOUT); // Transaction : Activation US
                busI2C.Dispose(); // Déconnexion virtuelle de l'objet SRF08 du bus I2C
            }

            /// <summary>
            /// Returns the value contained in a register
            /// et +
            /// </summary>
            /// <param name="RegisterNumber">The register number</param>
            /// <returns></returns>
            private byte GetRegister(Registers RegisterNumber)
            {
                // Buffer d'écriture
                byte[] outBuffer = new byte[] { (byte)RegisterNumber };
                I2CDevice.I2CWriteTransaction writeTransaction = I2CDevice.CreateWriteTransaction(outBuffer);

                // Buffer de lecture
                byte[] inBuffer = new byte[1];
                I2CDevice.I2CReadTransaction readTransaction = I2CDevice.CreateReadTransaction(inBuffer);

                // Tableau des transactions
                I2CDevice.I2CTransaction[] transactions = new I2CDevice.I2CTransaction[] { writeTransaction, readTransaction };
                // Exécution des transactions
                busI2C = new I2CDevice(ConfigSRF08); // Connexion virtuelle du SRF08 au bus I2C

                if (busI2C.Execute(transactions, TRANSACTIONEXECUTETIMEOUT) != 0)
                {
                    // Success
                    //Debug.Print("Received the first data from at device " + busI2C.Config.Address + ": " + ((int)inBuffer[0]).ToString());            
                }
                else
                {
                    // Failed
                    //Debug.Print("Failed to execute transaction at device: " + busI2C.Config.Address + ".");
                }
                busI2C.Dispose(); // Déconnexion virtuelle de l'objet Lcd du bus I2C
                return inBuffer[0];
            }
        } 
}