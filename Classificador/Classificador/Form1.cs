using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DIPLi;

namespace Classificador
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private Imagem Imagem;
        private Imagem Normalizada;
        private Imagem Segmentada;
        private Imagem Mediana;
        private Imagem Recortada;

        private object Abrir()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Title = "Selecionar Imagem";
            openFileDialog.InitialDirectory = "C:\\";
            openFileDialog.Filter = "Images (*.BMP;*.JPG;*.GIF,*.PNG,*.TIFF)|*.BMP;*.JPG;*.GIF;*.PNG;*.TIFF|" + "All files (*.*)|*.*";
            openFileDialog.CheckFileExists = true;
            openFileDialog.CheckPathExists = true;
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.ReadOnlyChecked = true;
            openFileDialog.ShowReadOnly = true;
            openFileDialog.FileName = "";

            DialogResult dr = openFileDialog.ShowDialog();
            Imagem image;
            if(dr == System.Windows.Forms.DialogResult.OK)
            {
                string file = openFileDialog.FileName.ToString();
                image = new Imagem(file);
                return image;
            }
            else
            {
                return false;
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                Imagem = (Imagem)Abrir();
                pictureBox1.Image = Imagem.ToBitmap();
                NormalizacaoDeCor();
                Segmentar();
                double[] vetor = new double[9];

                Imagem R = new Imagem(Segmentada.Largura, Segmentada.Altura);

                for (int i = 2; i < Segmentada.Altura - 2; i++)
                {
                    for (int j = 2; j < Segmentada.Largura - 2; j++)
                    {
                        vetor[0] = (Segmentada[i - 1, j - 1]);
                        vetor[1] = (Segmentada[i - 1, j]);
                        vetor[2] = (Segmentada[i - 1, j + 1]);
                        vetor[3] = (Segmentada[i, j - 1]);
                        vetor[4] = (Segmentada[i, j + 1]);
                        vetor[5] = (Segmentada[i + 1, j - 1]);
                        vetor[6] = (Segmentada[i + 1, j]);
                        vetor[7] = (Segmentada[i + 1, j + 1]);
                        vetor[8] = (Segmentada[i, j]);

                        vetor = quickSort(vetor);

                        R[i, j] = vetor[4];
                    }
                }
                Mediana = R;
                pictureBox4.Image = R.ToBitmap();
                Recorte();
            }
            catch (Exception)
            {
                MessageBox.Show("Houve um erro na seleção de imagem", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void Recorte()
        {
            Recortada = new Imagem(Imagem.Largura, Imagem.Altura, TipoImagem.Colorida);
            for (int i = 0; i < Mediana.Altura; i++)
            {
                for (int j = 0; j < Mediana.Largura; j++)
                {
                    for(int c = 0; c < 3; c++)
                    {
                        if (Mediana[i, j] == 255)
                        {
                            Recortada[i, j, c] = 0;
                        }
                        else
                        {
                            Recortada[i, j, c] = Imagem[i, j, c];
                        }
                    }
                }
            }
            pictureBox5.Image = Recortada.ToBitmap();
        }

        private void NormalizacaoDeCor()
        {
            Normalizada = new Imagem(Imagem.Largura, Imagem.Altura, Imagem.Tipo);

            double total = 0;
            double vermelho = 0;
            double verde = 0;
            double azul = 0;

            for (int a = 0; a < Imagem.Altura; a++)
            {
                for (int l = 0; l < Imagem.Largura; l++)
                {

                    total = Imagem[a, l, 0] + Imagem[a, l, 1] + Imagem[a, l, 2];
                    vermelho = (Imagem[a, l, 0] / total) * 255;
                    verde = (Imagem[a, l, 1] / total) * 255;
                    azul = (Imagem[a, l, 2] / total) * 255;

                    Normalizada[a, l, 0] = vermelho;
                    Normalizada[a, l, 1] = verde;
                    Normalizada[a, l, 2] = azul;
                }
            }
            pictureBox2.Image = Normalizada.ToBitmap();
        }

        private void Segmentar()
        {
            Segmentada = new Imagem(Imagem.Largura, Imagem.Altura, TipoImagem.Monocromatica);

            for (int a = 0; a < Imagem.Altura; a++)
            {
                for (int l = 0; l < Imagem.Largura; l++)
                {
                    double max = Math.Max(Math.Max(Normalizada[a, l, 0], Normalizada[a, l, 1]), Normalizada[a, l, 2]);
                    double min = Math.Min(Math.Min(Normalizada[a, l, 0], Normalizada[a, l, 1]), Normalizada[a, l, 2]);
                    double modulo = Normalizada[a, l, 0] - Normalizada[a, l, 1];
                    if (modulo < 0)
                    {
                        modulo = modulo * -1;
                    }
                    if (Normalizada[a, l, 0] > 95 && Normalizada[a, l, 1] > 40 && Normalizada[a, l, 2] > 20 && max - min > 15 && modulo > 15 && Normalizada[a, l, 0] > Normalizada[a, l, 1] && Normalizada[a, l, 0] > Normalizada[a, l, 2] && Normalizada[a, l, 1] > Normalizada[a, l, 2])
                    {
                        Segmentada[a, l] = 0;
                    }
                    else
                    {
                        Segmentada[a, l] = 255;
                    }
                }
            }
            pictureBox3.Image = Segmentada.ToBitmap();
        }

        public static double[] quickSort(double[] vetor)
        {
            int inicio = 0;
            int fim = vetor.Length - 1;

            quickSort(vetor, inicio, fim);

            return vetor;
        }

        private static void quickSort(double[] vetor, int inicio, int fim)
        {
            if (inicio < fim)
            {
                double p = vetor[inicio];
                int i = inicio + 1;
                int f = fim;

                while (i <= f)
                {
                    if (vetor[i] <= p)
                    {
                        i++;
                    }
                    else if (p < vetor[f])
                    {
                        f--;
                    }
                    else
                    {
                        double troca = vetor[i];
                        vetor[i] = vetor[f];
                        vetor[f] = troca;
                        i++;
                        f--;
                    }
                }

                vetor[inicio] = vetor[f];
                vetor[f] = p;

                quickSort(vetor, inicio, f - 1);
                quickSort(vetor, f + 1, fim);
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            Imagem = (Imagem)Abrir();
            pictureBox6.Image = Imagem.ToBitmap();
        }
    }
}
