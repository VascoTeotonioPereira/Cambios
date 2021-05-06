namespace Cambios.Modelos.Serviços
{
    using System.Windows.Forms;
    class DialogService
    {
        public void ShowMessage(string title, string message)
        {
            MessageBox.Show(message, title);
        }
    }
}
