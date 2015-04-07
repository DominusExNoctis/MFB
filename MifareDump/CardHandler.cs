using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MifareDump
{
    class CardHandler
    {
                                    //Global Reader Variable
        int retval;                                             //Return Value                            //Boolean variable to check the authentication
        Byte[] ATR = new Byte[33];                              //Array stores Card ATR                                        //Stores the card type
        Byte[] sendBuffer = new Byte[255];                        //Send Buffer in SCardTransmit
        Byte[] receiveBuffer = new Byte[255];                   //Receive Buffer in SCardTransmit
        int sendbufferlen, receivebufferlen;                    //Send and Receive Buffer length in SCardTransmit
        Byte bcla;                                             //Class Byte
        Byte bins;                                             //Instruction Byte
        Byte bp1;                                              //Parameter Byte P1
        Byte bp2;                                              //Parameter Byte P2
        Byte len;                                              //Lc/Le Byte
        Byte[] data = new Byte[255];                            //Data Bytes                                     //The maximum amount of time to wait for an action
        //String keych;                                           //Stores the string in key textbox
        int discarded;                                          //Stores the number of discarded character
       
        public void CargarLlaves(IntPtr hCard,List<string> llaves)
        {
            HiDWinscard.SCARD_IO_REQUEST sioreq;
            sioreq.dwProtocol = 0x2;
            sioreq.cbPciLength = 8;
            HiDWinscard.SCARD_IO_REQUEST rioreq;
            rioreq.cbPciLength = 8;
            rioreq.dwProtocol = 0x2;

            byte pos = 0;

            foreach (var llave in llaves)
            {
                Byte[] str3 = HexToBytenByteToHex.GetBytes(llave, out discarded); //Encoding.ASCII.GetBytes(keych1);    
                bcla = 0xFF;
                bins = 0x82;
                bp1 = 0x20;
                bp2 = pos;
                len = 0x6;
                sendBuffer[0] = bcla;
                sendBuffer[1] = bins;
                sendBuffer[2] = bp1;
                sendBuffer[3] = bp2;
                sendBuffer[4] = len;
                for (int k = 0; k <= str3.Length - 1; k++)
                    sendBuffer[k + 5] = str3[k];
                sendbufferlen = 0xB;
                receivebufferlen = 255;
                retval = HID.SCardTransmit(hCard, ref sioreq, sendBuffer, sendbufferlen, ref rioreq, receiveBuffer, ref receivebufferlen);
                if (retval == 0)
                {
                    if ((receiveBuffer[receivebufferlen - 2] == 0x90) && (receiveBuffer[receivebufferlen - 1] == 0))
                    {
                        Console.WriteLine("Llave {0} cargada en posicion {1}", llave, pos);

                    }
                    else
                    {
                        Console.WriteLine("Llave {0} con error al cargar en lector", llave);
                    }
                }
                else
                {

                }
                pos++;
            }

            //Autenticacion(hCard);
           Dump(hCard);
           
        }

        public void Dump(IntPtr hCard)
        {
            String read_str;
            HiDWinscard.SCARD_IO_REQUEST sioreq;
            sioreq.dwProtocol = 0x2;
            sioreq.cbPciLength = 8;
            HiDWinscard.SCARD_IO_REQUEST rioreq;
            rioreq.cbPciLength = 8;
            rioreq.dwProtocol = 0x2;
            StringBuilder sb = new StringBuilder();

            Console.WriteLine("================================");
            Console.WriteLine("=========== SECTOR 0 ===========");
            Console.WriteLine("================================");

            sb.AppendLine("================================");
            sb.AppendLine("=========== SECTOR 0 ===========");
            sb.AppendLine("================================");

            byte c = 1;
            byte sec = 0;
            byte keynum = 0;

            for (byte i = 0; i < 64; i++)
            {

                bcla = 0xFF;
                bins = 0x86;
                bp1 = 0x0;
                bp2 = 0x0;//'currentBlock
                len = 0x5;
                sendBuffer[0] = bcla;
                sendBuffer[1] = bins;
                sendBuffer[2] = bp1;
                sendBuffer[3] = bp2;

                sendBuffer[4] = len;
                sendBuffer[5] = 0x1;           //Version
                sendBuffer[6] = 0x0;           //Address MSB
                sendBuffer[7] = i;  //Address LSB

                sendBuffer[8] = 0x60; //Key Type A
                sendBuffer[9] = keynum;

                

                sendbufferlen = 0xA;
                receivebufferlen = 255;
                retval = HID.SCardTransmit(hCard, ref sioreq, sendBuffer, sendbufferlen, ref rioreq, receiveBuffer, ref receivebufferlen);
                if (retval == 0)
                {
                    if ((receiveBuffer[receivebufferlen - 2] == 0x90) && (receiveBuffer[receivebufferlen - 1] == 0))
                    {
                        //Console.WriteLine("Bloque {0} Autenticado", i);
                    }
                    else
                    {
                        Console.WriteLine("Bloque {0} Fallo", i);
                        sb.AppendLine(string.Format("Bloque {0} Fallo", i));
                    }
                }



                bcla = 0xFF;
                bins = 0xB0;
                bp1 = 0x0;
                bp2 = i;
                sendBuffer[0] = bcla;
                sendBuffer[1] = bins;
                sendBuffer[2] = bp1;
                sendBuffer[3] = bp2;
                sendBuffer[4] = 0x0;
                sendbufferlen = 0x5;
                receivebufferlen = 0x12;

                retval = HID.SCardTransmit(hCard, ref sioreq, sendBuffer, sendbufferlen, ref rioreq, receiveBuffer, ref receivebufferlen);
                if (retval == 0)
                {
                    if ((receiveBuffer[receivebufferlen - 2] == 0x90) && (receiveBuffer[receivebufferlen - 1] == 0))
                    {

                        read_str = HexToBytenByteToHex.ToString(receiveBuffer);
                        Console.WriteLine(read_str.Substring(0, ((int)(receivebufferlen - 2)) * 2));
                        sb.AppendLine(read_str.Substring(0, ((int)(receivebufferlen - 2)) * 2));
                    }
                    else
                    {
                        Console.WriteLine("NULL");
                        sb.AppendLine("NULL");
                    }
                }
                
                if (c % 4 == 0 && c != 64)
                {
                    keynum += 2;
                    sec++;
                    Console.WriteLine("================================");
                    Console.WriteLine("=========== SECTOR {0} ===========", sec);
                    Console.WriteLine("================================");
                    sb.AppendLine("================================");
                    sb.AppendLine(string.Format("=========== SECTOR {0} ===========",sec));
                    sb.AppendLine("================================");
                }
                c++;
            }

            File.WriteAllText("DumpBIP.txt", sb.ToString());
        }
    }
}
