using System;
using System.Windows.Forms;
using ChessUI;  // Aseg�rate de tener la referencia correcta al namespace del formulario

namespace ChessSimulator
{
    static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicaci�n.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ChessForm());  // Inicia el formulario principal de ChessForm
        }
    }
}
