﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MifareDump
{
    class FileHandler
    {
        public static List<string> ObtenerLlaves()
        {
            List<string> llaves = new List<string>();
            FileInfo fi = new FileInfo("KeysBip.txt");
            string linea;
            string[] tmp;

            if (fi.Exists)
            {
                StreamReader sr = fi.OpenText();
                while ((linea = sr.ReadLine()) != null)
                {
                    tmp = linea.Split(';');
                    llaves.Add(tmp[0]);
                    llaves.Add(tmp[1]);
                }
                sr.Close();
            }


            return llaves;
        }

        public static List<string> CargarArchivoClonacion()
        {
            List<string> bloques = new List<string>();
            FileInfo fi = new FileInfo("StagingFile.txt");
            string bloque;
            if (fi.Exists)
            {
                StreamReader sr = fi.OpenText();
                while ((bloque = sr.ReadLine()) != null)
                {
                    bloques.Add(bloque);
                }
                sr.Close();
            }

            return bloques;
        }
    }
}
