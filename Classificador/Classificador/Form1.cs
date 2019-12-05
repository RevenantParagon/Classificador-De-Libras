using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
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

        private Imagem Molde;
        /// <summary>
        /// Abre uma caixa de seleção para selecionar uma imagem e converte-la em um objeto tipo Imagem da Biblioteca DIPLi
        /// </summary>
        /// <returns></returns>
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
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                string file = openFileDialog.FileName.ToString();
                image = new Imagem(file);
                Bitmap bit = ResizeImage(image.ToBitmap(), 85, 112);
                image = new Imagem(bit);
                return image;
            }
            else
            {
                return false;
            }
        }

        Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);

                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }


        private void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                Imagem = (Imagem)Abrir();
                label1.Text = "";
                pictureBox1.Image = Imagem.ToBitmap();
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
                Recorte();
            }
            catch (Exception)
            {
                MessageBox.Show("Houve um erro na seleção de imagem", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        /// <summary>
        /// Recorta a imagem a partir de uma mascara
        /// </summary>
        private void Recorte()
        {
            Recortada = new Imagem(Imagem.Largura, Imagem.Altura, TipoImagem.Colorida);
            for (int i = 0; i < Mediana.Altura; i++)
            {
                for (int j = 0; j < Mediana.Largura; j++)
                {
                    for (int c = 0; c < 3; c++)
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
        }

        /// <summary>
        /// Cria uma mascara binaria a partir da imagem
        /// </summary>
        private void Segmentar()
        {
            Segmentada = new Imagem(Imagem.Largura, Imagem.Altura, TipoImagem.Monocromatica);

            for (int a = 0; a < Imagem.Altura; a++)
            {
                for (int l = 0; l < Imagem.Largura; l++)
                {
                    double max = Math.Max(Math.Max(Imagem[a, l, 0], Imagem[a, l, 1]), Imagem[a, l, 2]);
                    double min = Math.Min(Math.Min(Imagem[a, l, 0], Imagem[a, l, 1]), Imagem[a, l, 2]);
                    double modulo = Imagem[a, l, 0] - Imagem[a, l, 1];
                    if (modulo < 0)
                    {
                        modulo = modulo * -1;
                    }
                    if (max - min > 15 && modulo > 9 && Imagem[a, l, 0] > Imagem[a, l, 1] && Imagem[a, l, 0] > Imagem[a, l, 2] && Imagem[a, l, 1] > Imagem[a, l, 2])
                    {
                        Segmentada[a, l] = 0;
                    }
                    else
                    {
                        Segmentada[a, l] = 255;
                    }
                }
            }
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
            int erro = 0;
            int certo = 0;
            Imagem Molde;

            int porcentagem = 0;
            Molde = new Imagem("LetraA.bmp");
            for (int j = 0; j < Molde.Altura; j++)
            {
                for (int k = 0; k < Molde.Largura; k++)
                {
                    if (Molde[j, k] == 0 && Recortada[j, k] != 0)
                    {
                        certo++;
                    }
                    else if (Molde[j, k] == 0 && Recortada[j, k] == 0)
                    {
                        erro++;
                    }
                }
            }
            porcentagem = (certo * 100) / (certo + erro);
            if (porcentagem >= 90)
            {
                label1.Text = "Letra A";
            }

            erro = 0;
            certo = 0;
            porcentagem = 0;
            Molde = new Imagem("LetraB.bmp");
            for (int j = 0; j < Molde.Altura; j++)
            {
                for (int k = 0; k < Molde.Largura; k++)
                {
                    if (Molde[j, k] == 0 && Recortada[j, k] != 0)
                    {
                        certo++;
                    }
                    else if (Molde[j, k] == 0 && Recortada[j, k] == 0)
                    {
                        erro++;
                    }
                }
            }
            porcentagem = (certo * 100) / (certo + erro);
            if (porcentagem >= 90)
            {
                label1.Text = "Letra B";
            }

            erro = 0;
            certo = 0;
            porcentagem = 0;
            Molde = new Imagem("LetraC.bmp");
            for (int j = 0; j < Molde.Altura; j++)
            {
                for (int k = 0; k < Molde.Largura; k++)
                {
                    if (Molde[j, k] == 0 && Recortada[j, k] != 0)
                    {
                        certo++;
                    }
                    else if (Molde[j, k] == 0 && Recortada[j, k] == 0)
                    {
                        erro++;
                    }
                }
            }
            porcentagem = (certo * 100) / (certo + erro);
            if (porcentagem >= 90)
            {
                label1.Text = "Letra C";
            }

            //    erro = 0;
            //    certo = 0;
            //    porcentagem = 0;
            //    Molde = new Imagem("c2.bmp");
            //    for (int j = 0; j < Molde.Altura; j++)
            //    {
            //        for (int k = 0; k < Molde.Largura; k++)
            //        {
            //            if (Molde[j, k] == 0 && Recortada[j, k] != 0)
            //            {
            //                certo++;
            //            }
            //            else if (Molde[j, k] == 0 && Recortada[j, k] == 0)
            //            {
            //                erro++;
            //            }
            //        }
            //    }
            //    porcentagem = (certo * 100) / (certo + erro);
            //    if (porcentagem >= 95)
            //    {
            //        label1.Text = "Letra C";
            //    }

            //    erro = 0;
            //    certo = 0;
            //    porcentagem = 0;
            //    Molde = new Imagem("d.bmp");
            //    for (int j = 0; j < Molde.Altura; j++)
            //    {
            //        for (int k = 0; k < Molde.Largura; k++)
            //        {
            //            if (Molde[j, k] == 0 && Recortada[j, k] != 0)
            //            {
            //                certo++;
            //            }
            //            else if (Molde[j, k] == 0 && Recortada[j, k] == 0)
            //            {
            //                erro++;
            //            }
            //        }
            //    }
            //    porcentagem = (certo * 100) / (certo + erro);
            //    if (porcentagem >= 93)
            //    {
            //        label1.Text = "Letra D";

            //    }

            //    erro = 0;
            //    certo = 0;
            //    porcentagem = 0;
            //    Molde = new Imagem("e.bmp");
            //    for (int j = 0; j < Molde.Altura; j++)
            //    {
            //        for (int k = 0; k < Molde.Largura; k++)
            //        {
            //            if (Molde[j, k] == 0 && Recortada[j, k] != 0)
            //            {
            //                certo++;
            //            }
            //            else if (Molde[j, k] == 0 && Recortada[j, k] == 0)
            //            {
            //                erro++;
            //            }
            //        }
            //    }
            //    porcentagem = (certo * 100) / (certo + erro);
            //    if (porcentagem >= 95)
            //    {
            //        label1.Text = "Letra E";
            //    }
            //
        }

    }
}
