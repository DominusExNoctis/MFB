using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Configuration;
using MifareDump;
using System.Runtime.InteropServices;

namespace CS_ControlArmamento_CapturaRFID
{

    public class LectorHID
    {
        [DllImport("msvcrt.dll")]
        static extern bool system(string str);

        uint dwscope;                                           //Scope of the resource manager context
        IntPtr hContext;                                        //Context Handle value
        int retval;                                             //Return Value
        String ReaderList;                                      //List Of Reader
        String readerName;                                      //Global Reader Variable
        private System.Timers.Timer timer;                      //Object of the Timer
        HiDWinscard.SCARD_READERSTATE ReaderState;              //Object of SCARD_READERSTATE
        IntPtr hCard;                                           //Card handle
        IntPtr protocol;                                        //Protocol used currently
        Byte[] ATR = new Byte[33];                              //Array stores Card ATR
        int card_Type;                                          //Stores the card type
        int value_Timeout;                                      //The maximum amount of time to wait for an action
        uint ReaderCount;                                       //Count for number of readers
        Byte bcla;                                             //Class Byte
        Byte bins;                                             //Instruction Byte
        Byte bp1;                                              //Parameter Byte P1
        Byte bp2;                                              //Parameter Byte P2
        Byte len;                                              //Lc/Le Byte
        Byte[] data = new Byte[255];                            //Data Bytes
        Byte[] sendBuffer = new Byte[255];                        //Send Buffer in SCardTransmit
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x16)]
        // public byte receiveBuffer;
        Byte[] receiveBuffer = new Byte[255];                   //Receive Buffer in SCardTransmit
        int sendbufferlen, receivebufferlen;                    //Send and Receive Buffer length in SCardTransmit

        bool activar;

        public LectorHID()
        {
           
            uint pcchReaders = 0;
            int nullindex = -1;
            char nullchar = (char)0;
            dwscope = 2;

            // Establish context.
            retval = HID.SCardEstablishContext(dwscope, IntPtr.Zero, IntPtr.Zero, out hContext);
            retval = HID.SCardListReaders(hContext, null, null, ref pcchReaders);
            byte[] mszReaders = new byte[pcchReaders];

            // Fill readers buffer with second call.
            retval = HID.SCardListReaders(hContext, null, mszReaders, ref pcchReaders);

            // Populate List with readers.
            string currbuff = Encoding.ASCII.GetString(mszReaders);
            
            int len = (int)pcchReaders;

            if (len > 0)
            {
                nullindex = currbuff.IndexOf(nullchar);   // Get null end character.
                string reader = currbuff.Substring(0, nullindex);
                len = len - (reader.Length + 1);
                currbuff = currbuff.Substring(nullindex + 1, len);
                readerName = currbuff;
            }

            try
            {
                dwscope = 2;
                if (readerName != "" && readerName != null)
                {
                    retval = HID.SCardEstablishContext(dwscope, IntPtr.Zero, IntPtr.Zero, out hContext);
                    if (retval == 0)
                    {
                        Console.WriteLine("Contexto Establecido");
                    }
                    else
                    {
                        Console.WriteLine("Error Numero:{0}!!!",retval);
                    }
                }
                else
                {
                    Console.WriteLine("Error Numero:{0}!!!", retval);
                }
            }
            catch { }

        }

        public void Iniciar()
        {
            

            timer = new System.Timers.Timer(1000);
            // Hook up the Elapsed event for the timer.
            timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            timer.Enabled = true;
            timer.Start();
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {

            timerWorkItem();
           
        }

        private void timerWorkItem()
        {

            retval = HID.SCardConnect(hContext, readerName, HiDWinscard.SCARD_SHARE_SHARED, HiDWinscard.SCARD_PROTOCOL_T1,
                                 ref hCard, ref protocol
                                  );
            ReaderState.RdrName = readerName;
            ReaderState.RdrCurrState = HiDWinscard.SCARD_STATE_UNAWARE;
            ReaderState.RdrEventState = 0;
            ReaderState.UserData = "Mifare Card";
            value_Timeout = 0;
            ReaderCount = 1;

            if (retval == 0)
            {
                timer.Enabled = false;
                Console.WriteLine("Tarjeta Conectada");

                CardHandler card = new CardHandler();
                card.CargarLlaves(hCard,FileHandler.ObtenerLlaves());

                system("pause");
            }

            //retval = HID.SCardGetStatusChange(hContext, value_Timeout, ref ReaderState, ReaderCount);

            //if ((ReaderState.ATRLength == 0) || (retval != 0))
            //{
            //    Console.WriteLine("Tarjeta Desconectada");
            //    timer.Enabled = true;
            //}

            else if (retval != 0)
            {
                timer.Enabled = true;
            }
        }

        private void ATR_UID(int card_type)
        {
            HiDWinscard.SCARD_IO_REQUEST sioreq;
            sioreq.dwProtocol = 0x2;
            sioreq.cbPciLength = 8;
            HiDWinscard.SCARD_IO_REQUEST rioreq;
            rioreq.cbPciLength = 8;
            rioreq.dwProtocol = 0x2;

            String uid_temp;
            String atr_temp;
            String s;
            atr_temp = "";
            uid_temp = "";
            s = "";
            StringBuilder hex = new StringBuilder(ReaderState.ATRValue.Length * 2);
            foreach (byte b in ReaderState.ATRValue)
                hex.AppendFormat("{0:X2}", b);
            atr_temp = hex.ToString();
            atr_temp = atr_temp.Substring(0, ((int)(ReaderState.ATRLength)) * 2);



            for (int k = 0; k <= ((ReaderState.ATRLength) * 2 - 1); k += 2)
            {
                s = s + atr_temp.Substring(k, 2) + " ";
            }

            atr_temp = s;

            bcla = 0xFF;
            bins = 0xCA;
            bp1 = 0x0;
            bp2 = 0x0;
            len = 0x0;
            sendBuffer[0] = bcla;
            sendBuffer[1] = bins;
            sendBuffer[2] = bp1;
            sendBuffer[3] = bp2;
            sendBuffer[4] = len;
            sendbufferlen = 0x5;
            receivebufferlen = 255;
            retval = HID.SCardTransmit(hCard, ref sioreq, sendBuffer, sendbufferlen, ref rioreq, receiveBuffer, ref receivebufferlen);
            if (retval == 0)
            {
                if ((receiveBuffer[receivebufferlen - 2] == 0x90) && (receiveBuffer[receivebufferlen - 1] == 0))
                {
                    StringBuilder hex1 = new StringBuilder((receivebufferlen - 2) * 2);
                    foreach (byte b in receiveBuffer)
                        hex1.AppendFormat("{0:X2}", b);
                    uid_temp = hex1.ToString();
                    uid_temp = uid_temp.Substring(0, ((int)(receivebufferlen - 2)) * 2);
                }
                else
                {
                    ;
                }
            }
            else
            {
                
                timer.Enabled = false;
            }
            if (uid_temp == "")
            {
            }
            else
            {
                Console.WriteLine(uid_temp);
                Int64 decValue = Convert.ToInt64(InvertirHEX(uid_temp), 16);
                string id_RFID = decValue.ToString().PadLeft(25,'0');
                
                Console.WriteLine(id_RFID);

            }
        }

        private string InvertirHEX(string hexOriginal)
        {
            string HexMod = string.Empty;

            for (int i = 0; i < hexOriginal.Length - 1; i+=2)
            {
                HexMod = hexOriginal.Substring(i, 2) + HexMod;
            }
            return HexMod;
        }
    }
}
