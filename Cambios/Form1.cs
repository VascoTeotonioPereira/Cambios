namespace Cambios
{
    using Modelos.Serviços;
    using Modelos;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    public partial class Form1 : Form
    {
        #region Atributos

        private List<Rate> Rates;

        private NetworkService netWorkService;

        private ApiService apiService;

        private DialogService dialogService;

        private DataService dataService;
        #endregion

        public Form1()
        {
            InitializeComponent();
            netWorkService = new NetworkService();
            apiService = new ApiService();
            dialogService = new DialogService();
            dataService = new DataService();
            LoadRates();
        }

        private async void LoadRates()
        {
            bool load;

            labelResultado.Text = "A atualizar taxas...";

            var connection = netWorkService.CheckConnection();

            if (!connection.IsSuccess)
            {
                LoadLocalRates();
                load = false;
            }
            else
            {
                await LoadApiRates();
                load = true;
            }

            if (Rates.Count == 0)
            {
                labelResultado.Text = "Não há ligação á Internet" + Environment.NewLine + "e não foram préviamente carregadas as taxas." + Environment.NewLine + "Tente mais tarde!";

                labelStatus.Text = "Primeira inicialização deverá ter ligação á Internet.";

                return;
            }

            comboBoxOrigem.DataSource = Rates;

            //Igual ao override ToString()
            //comboBoxOrigem.DisplayMember = "Name";

            //Corrige bug da microsoft
            comboBoxDestino.BindingContext = new BindingContext();

            comboBoxDestino.DataSource = Rates;


            

            labelResultado.Text = "Taxas atualizadas...";

            if (load)
            {
                labelStatus.Text = string.Format("Taxas carregadas da internet em {0:F}", DateTime.Now);
            }
            else
            {
                labelStatus.Text = string.Format("Taxas carregadas da base de dados.");
            }

            progressBar1.Value = 100;

            buttonConverter.Enabled = true;
            buttonTroca.Enabled = true;
        }

        private void LoadLocalRates()
        {
            Rates = dataService.GetData();
        }

        private async Task LoadApiRates()
        {
            progressBar1.Value = 0;

            var response = await apiService.GetRates("http://cambios.somee.com", "/api/rates");

            Rates = (List<Rate>)response.Result;

            dataService.DeleteData();

            dataService.SaveData(Rates);
        }

        private void buttonConverter_Click(object sender, EventArgs e)
        {
            Converter();
        }

        private void Converter()
        {
            if (string.IsNullOrEmpty(textBoxValor.Text))
            {
                dialogService.ShowMessage("Erro", "Insira um valor a converter.");
                return;
            }

            decimal valor;
            if (!decimal.TryParse(textBoxValor.Text,out valor))
            {
                dialogService.ShowMessage("Erro de conversão", "O valor terá que ser numérico.");
                textBoxValor.Text = "";
                return;
            }

            if (comboBoxOrigem.SelectedItem == null)
            {
                dialogService.ShowMessage("Erro", "Tem que escolher uma moeda a converter.");
                return;
            }

            if (comboBoxDestino.SelectedItem == null)
            {
                dialogService.ShowMessage("Erro", "Tem que escolher uma moeda de destino.");
                return;
            }

            var taxaOrigem = (Rate)comboBoxOrigem.SelectedItem;
            var taxaDestino = (Rate)comboBoxDestino.SelectedItem;

            var valorConvertido = valor / (decimal)taxaOrigem.TaxRate * (decimal)taxaDestino.TaxRate;

            labelResultado.Text =string.Format("{0} {1:C2} = {2} {3:C2}", taxaOrigem.Code, valor, taxaDestino.Code, valorConvertido);
        }

        private void buttonTroca_Click(object sender, EventArgs e)
        {
            Trocar();
        }

        private void Trocar()
        {
            var aux = comboBoxOrigem.SelectedItem;
            comboBoxOrigem.SelectedItem = comboBoxDestino.SelectedItem;
            comboBoxDestino.SelectedItem = aux;
            Converter();
        }
    }
}
