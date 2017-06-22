using System;
using System.Threading;
using Microsoft.SPOT.Hardware;


namespace testMicroToolsKit
{
    namespace Hardware
    {
        namespace IO
        {
            /// <summary>
            /// Devantech SRF08 UltraSonic Ranger class
            /// </summary>
            /// <remarks>
            /// You may have some additional information about this class on http://webge.github.io/SRF08/
            /// </remarks>
            public class SRF08
            {
                /// <summary>
                /// Transaction time out = 1s before throwing System.IO.IOException
                /// </summary>                                          
                private UInt16 _transactionTimeOut = 1000;

                /// <summary>
                /// Slave Adress and frequency configuration
                /// </summary>
                private I2CDevice i2cBus = null;
                private I2CDevice.Configuration config = null;

                /// <summary>
                /// 7-bit Slave Adress
                /// </summary>
                private UInt16 _sla;

                private MeasuringUnits unit = MeasuringUnits.undefined;

                /// <summary>
                /// Get or set time before System IO Exception if transaction failed (in ms).
                /// </summary>
                /// <remarks>
                /// 1000ms by default
                /// </remarks>
                public UInt16 TransactionTimeOut
                {
                    get
                    {
                        return _transactionTimeOut;
                    }

                    set
                    {
                        _transactionTimeOut = value;
                    }
                }
                /// <summary>
                /// Slave Adress Get Access
                /// </summary>
                public UInt16 SLA
                {
                    get
                    {
                        return _sla;
                    }
                }

                /// <summary>
                /// SRF08 Registers list
                /// </summary>
                private enum Registers
                {
                    /// <summary>
                    /// SRF08 software revision. Read only.
                    /// </summary>
                    SoftRev = 0,
                    /// <summary>
                    /// Command register is used to start a ranging session. Write only.
                    /// </summary>
                    Command = 0,
                    /// <summary>
                    /// This data is updated every time a new ranging command has completed and can be read when range data is read. Read only.
                    /// </summary>
                    LightSensor = 1,
                    /// <summary>
                    /// Max Gain Register ((default 31). Write only
                    /// </summary>
                    /// <remarks>
                    /// 
                    /// </remarks>
                    Gain = 1,
                    /// <summary>
                    /// 8-bit high of  16-bit unsigned result from the latest ranging. Read only.
                    /// The meaning of this value depends on the  command used, and is either the range in inches, 
                    /// or the range in cm or the flight time in uS. A value of zero indicates that no objects were detected
                    /// </summary>
                    _1stEchoH = 2,
                    /// <summary>
                    /// Changing the range (default 255)
                    /// </summary>
                    /// <remarks>
                    /// The range is ((Range Register x 43mm) + 43mm) 
                    /// </remarks>
                    Range = 2,
                    /// <summary>
                    ///  8-bit low of  16-bit unsigned result from the latest ranging
                    /// </summary>
                    _1stEchoL = 3
                }
                /// <summary>
                /// SRF08 ranging Mode and Unity
                /// </summary>
                public enum MeasuringUnits
                {
                    /// <summary>
                    /// Ranging Mode and result in inches
                    /// </summary>
                    inches_InRangingMode,
                    /// <summary>
                    /// Ranging Mode and result in centimeters
                    /// </summary>
                    centimeters_InRangingMode,
                    /// <summary>
                    /// Ranging Mode and result in microseconds
                    /// </summary>
                    microseconds_InRangingMode,
                    /// <summary>
                    /// ANN Mode and result in inches
                    /// </summary>
                    inches_InANNMode,
                    /// <summary>
                    /// ANN Mode and result in centimeters
                    /// </summary>
                    centimeters_InANNMode,
                    /// <summary>
                    /// ANN Mode and result in microseconds
                    /// </summary>
                    microseconds_InANNMode, 
                    /// <summary>
                    /// Undefined
                    /// </summary>
                    undefined
                };

                /// <summary>
                /// Constructor with SLave Address = 0x70 and bus Frequency = 100kHz
                /// </summary>
                public SRF08()
                {
                    _sla = 0x70;
                    config = new I2CDevice.Configuration(0x70, 100);
                }
                /// <summary>
                /// Constructor with Bus Frequency = 100kHz
                /// </summary>
                /// <param name="SLA">The default shipped address of the SRF08 is 0x70. 
                ///  It can be changed by the user to any of 16 addresses (70 to 7F). 
                ///  </param>
                public SRF08(byte SLA)
                {
                    _sla = SLA;
                    config = new I2CDevice.Configuration(SLA, 100);
                }
                /// <summary>
                /// Constructor
                /// </summary>
                /// <param name="SLA">The default shipped address of the SRF08 is 0x70. 
                ///  It can be changed by the user to any of 16 addresses (70 to 7F). </param>
                /// <param name="Frequency">400kHz max</param>
                public SRF08(ushort SLA, UInt16 Frequency)
                {
                    _sla = SLA;
                    config = new I2CDevice.Configuration(SLA, Frequency);
                }

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
                public byte LightSensor
                {
                    get
                    {
                        byte lightSensor = GetRegister(Registers.LightSensor);
                        return lightSensor;
                    }
                }
                /// <summary>
                /// 1st Echo High Byte Get Access
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
                    byte command = (byte)(80 + (byte)units);

                    byte[] outBuffer = new byte[] { (byte)Registers.Command, command };
                    I2CDevice.I2CTransaction WriteUnit = I2CDevice.CreateWriteTransaction(outBuffer);

                    byte[] inBuffer = new byte[4];
                    I2CDevice.I2CTransaction ReadDist = I2CDevice.CreateReadTransaction(inBuffer);

                    I2CDevice.I2CTransaction[] T_WriteUnit = new I2CDevice.I2CTransaction[] { WriteUnit };
                    I2CDevice.I2CTransaction[] T_ReadDist = new I2CDevice.I2CTransaction[] { ReadDist };

                    i2cBus = new I2CDevice(config);
                    i2cBus.Execute(T_WriteUnit, _transactionTimeOut);
                    Thread.Sleep(75);
                    int transferred = i2cBus.Execute(T_ReadDist, _transactionTimeOut);
                    i2cBus.Dispose();

                    if (transferred != 0)
                    {
                        UInt16 range = (UInt16)((UInt16)(inBuffer[3] << 8) + inBuffer[2]);
                        return range;
                    }
                    else
                    {
                        throw new System.IO.IOException("I2CBus error " + _sla.ToString());

                    }
                }

                /// <summary>
                /// Only triggers a shot ulrasons
                /// </summary>
                /// <param name="units">unit of measure expected</param>
                public void TrigShotUS(MeasuringUnits units)
                {
                    this.unit = units;
                    byte commandByte = (byte)(80 + (byte)units);

                    byte[] outbuffer = new byte[] { (byte)Registers.Command, commandByte };
                    I2CDevice.I2CTransaction WriteUnit = I2CDevice.CreateWriteTransaction(outbuffer);

                    I2CDevice.I2CTransaction[] T_WriteUnit = new I2CDevice.I2CTransaction[] { WriteUnit };

                    i2cBus = new I2CDevice(config);
                    int transferred = i2cBus.Execute(T_WriteUnit, _transactionTimeOut);
                    i2cBus.Dispose();
                    if (transferred != 0)
                    {;}
                    else
                        throw new System.IO.IOException("I2CBus error " + _sla.ToString());
                }

                /// <summary>
                /// Returns the value contained in a register
                /// </summary>
                /// <remarks>
                /// System.IO.IOException trowed with "I2CBus error SLA" message if TRANSACTION TIME OUT.
                /// </remarks>
                /// <param name="RegisterNumber">The register number</param>
                /// <returns>
                /// Value in register
                /// </returns>
                private byte GetRegister(Registers RegisterNumber)
                {
                    byte[] outBuffer = new byte[] { (byte)RegisterNumber };
                    I2CDevice.I2CWriteTransaction writeTransaction = I2CDevice.CreateWriteTransaction(outBuffer);

                    byte[] inBuffer = new byte[1];
                    I2CDevice.I2CReadTransaction readTransaction = I2CDevice.CreateReadTransaction(inBuffer);

                    I2CDevice.I2CTransaction[] transactions = new I2CDevice.I2CTransaction[] { writeTransaction, readTransaction };

                    i2cBus = new I2CDevice(config);
                    int transferred = i2cBus.Execute(transactions, _transactionTimeOut);
                    i2cBus.Dispose();

                    if (transferred != 0)
                        return inBuffer[0];
                    else
                        throw new System.IO.IOException("I2CBus error " + _sla.ToString());
                }
            }
        }
    }
}
