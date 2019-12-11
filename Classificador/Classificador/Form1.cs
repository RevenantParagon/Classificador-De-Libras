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
        private Imagem Segmentada;
        private Imagem Recortada;

        /// <summary>
        /// Abre uma caixa de seleção para selecionar uma imagem e converte-la em um objeto tipo Imagem da Biblioteca DIPLi
        /// </summary>
        /// <returns></returns>
        private object Abrir()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = false,
                Title = "Selecionar Imagem",
                InitialDirectory = "C:\\",
                Filter = "Images (*.BMP;*.JPG;*.GIF,*.PNG,*.TIFF)|*.BMP;*.JPG;*.GIF;*.PNG;*.TIFF|" + "All files (*.*)|*.*",
                CheckFileExists = true,
                CheckPathExists = true,
                FilterIndex = 2,
                RestoreDirectory = true,
                ReadOnlyChecked = true,
                ShowReadOnly = true,
                FileName = ""
            };

            DialogResult dr = openFileDialog.ShowDialog();
            Imagem image;
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                string file = openFileDialog.FileName.ToString();
                image = new Imagem(file);
                Bitmap bit = ResizeImage(image.ToBitmap(), 100, 100);
                image = new Imagem(bit);
                return image;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Redimensiona uma imagem
        /// </summary>
        /// <param name="image">Imagem a ser redimensionada</param>
        /// <param name="width">Largura</param>
        /// <param name="height">Altura</param>
        /// <returns></returns>
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

        private void Sort()
        {
            double[] vetor = new double[9];
            Imagem R = new Imagem(Segmentada.Largura, Segmentada.Altura);
            for (int i = 1; i < Segmentada.Altura - 1; i++)
            {
                for (int j = 1; j < Segmentada.Largura - 1; j++)
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
                    vetor = QuickSort(vetor);
                    R[i, j] = vetor[4];
                }
            }
            for (int i = 0; i < Segmentada.Altura; i++)
            {
                for (int j = 0; j < Segmentada.Largura; j++)
                {
                    if(i == 0 || i == Segmentada.Altura || j == 0 || j == Segmentada.Largura)
                    {
                        R[i ,j] = 0;
                    }
                }
            }
            Segmentada = R;
        }

        /// <summary>
        /// Delinear borda em azul
        /// </summary>
        private void DelinearBorda()
        {
            Imagem i = new Imagem(Recortada.Largura, Recortada.Altura, Recortada.Tipo);
            for (int l = 0; l < Recortada.Largura; l++)
            {
                for (int a = 0; a < Recortada.Altura; a++)
                {
                    if(Recortada[a, l, 0] == 0 && Recortada[a, l, 1] == 0 && Recortada[a, l, 2] == 0)
                    {
                        if(l == 0 && a == 0)
                        {
                            if(Recortada[a + 1, l,0] != 0 || Recortada[a + 1, l, 1] != 0 || Recortada[a + 1, l, 2] != 0)
                            {
                                i[a, l, 0] = 0;
                                i[a, l, 1] = 0;
                                i[a, l, 2] = 255;
                            }
                            else if(Recortada[a + 1, l + 1, 0] != 0 || Recortada[a + 1, l + 1, 1] != 0 || Recortada[a + 1, l + 1, 2] != 0)
                            {
                                i[a, l, 0] = 0;
                                i[a, l, 1] = 0;
                                i[a, l, 2] = 255;
                            }
                            else if (Recortada[a, l + 1, 0] != 0 || Recortada[a, l + 1, 1] != 0 || Recortada[a, l + 1, 2] != 0)
                            {
                                i[a, l, 0] = 0;
                                i[a, l, 1] = 0;
                                i[a, l, 2] = 255;
                            }
                        }
                        else if(l == 0 && a == Recortada.Altura)
                        {
                            if (i[a - 1, l, 0] != 0 || Recortada[a - 1, l, 1] != 0 || Recortada[a - 1, l, 2] != 0)
                            {
                                i[a, l, 0] = 0;
                                i[a, l, 1] = 0;
                                i[a, l, 2] = 255;
                            }
                            else if (Recortada[a - 1, l + 1, 0] != 0 || Recortada[a - 1, l + 1, 1] != 0 || Recortada[a - 1, l + 1, 2] != 0)
                            {
                                i[a, l, 0] = 0;
                                i[a, l, 1] = 0;
                                i[a, l, 2] = 255;
                            }
                            else if (Recortada[a, l + 1, 0] != 0 || Recortada[a, l + 1, 1] != 0 || Recortada[a, l + 1, 2] != 0)
                            {
                                i[a, l, 0] = 0;
                                i[a, l, 1] = 0;
                                i[a, l, 2] = 255;
                            }
                        }
                        else if (l == Recortada.Largura && a == 0)
                        {
                            if (Recortada[a + 1, l, 0] != 0 || Recortada[a + 1, l, 1] != 0 || Recortada[a + 1, l, 2] != 0)
                            {
                                i[a, l, 0] = 0;
                                i[a, l, 1] = 0;
                                i[a, l, 2] = 255;
                            }
                            else if (Recortada[a + 1, l - 1, 0] != 0 || Recortada[a + 1, l - 1, 1] != 0 || Recortada[a + 1, l - 1, 2] != 0)
                            {
                                i[a, l, 0] = 0;
                                i[a, l, 1] = 0;
                                i[a, l, 2] = 255;
                            }
                            else if (Recortada[a, l - 1, 0] != 0 || Recortada[a, l - 1, 1] != 0 || Recortada[a, l - 1, 2] != 0)
                            {
                                i[a, l, 0] = 0;
                                i[a, l, 1] = 0;
                                i[a, l, 2] = 255;
                            }
                        }
                        else if (l == Recortada.Largura && a == Recortada.Altura)
                        {
                            if (Recortada[a - 1, l, 0] != 0 || Recortada[a - 1, l, 1] != 0 || Recortada[a - 1, l, 2] != 0)
                            {
                                i[a, l, 0] = 0;
                                i[a, l, 1] = 0;
                                i[a, l, 2] = 255;
                            }
                            else if (Recortada[a - 1, l - 1, 0] != 0 || Recortada[a - 1, l - 1, 1] != 0 || Recortada[a - 1, l - 1, 2] != 0)
                            {
                                i[a, l, 0] = 0;
                                i[a, l, 1] = 0;
                                i[a, l, 2] = 255;
                            }
                            else if (Recortada[a, l - 1, 0] != 0 || Recortada[a, l - 1, 1] != 0 || Recortada[a, l - 1, 2] != 0)
                            {
                                i[a, l, 0] = 0;
                                i[a, l, 1] = 0;
                                i[a, l, 2] = 255;
                            }
                        }
                        else if (l == 0)
                        {
                            if (Recortada[a - 1, l, 0] != 0 || Recortada[a - 1, l, 1] != 0 || Recortada[a - 1, l, 2] != 0)
                            {
                                i[a, l, 0] = 0;
                                i[a, l, 1] = 0;
                                i[a, l, 2] = 255;
                            }
                            else if (Recortada[a + 1, l, 0] != 0 || Recortada[a + 1, l, 1] != 0 || Recortada[a + 1, l, 2] != 0)
                            {
                                i[a, l, 0] = 0;
                                i[a, l, 1] = 0;
                                i[a, l, 2] = 255;
                            }
                            else if (Recortada[a + 1, l + 1, 0] != 0 || Recortada[a + 1, l + 1, 1] != 0 || Recortada[a + 1, l + 1, 2] != 0)
                            {
                                i[a, l, 0] = 0;
                                i[a, l, 1] = 0;
                                i[a, l, 2] = 255;
                            }
                            else if (Recortada[a - 1, l + 1, 0] != 0 || Recortada[a - 1, l + 1, 1] != 0 || Recortada[a - 1, l + 1, 2] != 0)
                            {
                                i[a, l, 0] = 0;
                                i[a, l, 1] = 0;
                                i[a, l, 2] = 255;
                            }
                            else if (Recortada[a, l + 1, 0] != 0 || Recortada[a, l + 1, 1] != 0 || Recortada[a, l + 1, 2] != 0)
                            {
                                i[a, l, 0] = 0;
                                i[a, l, 1] = 0;
                                i[a, l, 2] = 255;
                            }
                        }
                        else if (l == Recortada.Largura)
                        {
                            if (Recortada[a - 1, l, 0] != 0 || Recortada[a - 1, l, 1] != 0 || Recortada[a - 1, l, 2] != 0)
                            {
                                i[a, l, 0] = 0;
                                i[a, l, 1] = 0;
                                i[a, l, 2] = 255;
                            }
                            else if (Recortada[a + 1, l, 0] != 0 || Recortada[a + 1, l, 1] != 0 || Recortada[a + 1, l, 2] != 0)
                            {
                                i[a, l, 0] = 0;
                                i[a, l, 1] = 0;
                                i[a, l, 2] = 255;
                            }
                            else if (Recortada[a + 1, l - 1, 0] != 0 || Recortada[a + 1, l - 1, 1] != 0 || Recortada[a + 1, l - 1, 2] != 0)
                            {
                                i[a, l, 0] = 0;
                                i[a, l, 1] = 0;
                                i[a, l, 2] = 255;
                            }
                            else if (Recortada[a - 1, l - 1, 0] != 0 || Recortada[a - 1, l - 1, 1] != 0 || Recortada[a - 1, l - 1, 2] != 0)
                            {
                                i[a, l, 0] = 0;
                                i[a, l, 1] = 0;
                                i[a, l, 2] = 255;
                            }
                            else if (Recortada[a, l - 1, 0] != 0 || Recortada[a, l - 1, 1] != 0 || Recortada[a, l - 1, 2] != 0)
                            {
                                i[a, l, 0] = 0;
                                i[a, l, 1] = 0;
                                i[a, l, 2] = 255;
                            }
                        }
                        else if (a == Recortada.Altura)
                        {
                            if (Recortada[a - 1, l, 0] != 0 || Recortada[a - 1, l, 1] != 0 || Recortada[a - 1, l, 2] != 0)
                            {
                                i[a, l, 0] = 0;
                                i[a, l, 1] = 0;
                                i[a, l, 2] = 255;
                            }
                            else if (Recortada[a, l + 1, 0] != 0 || Recortada[a, l + 1, 1] != 0 || Recortada[a, l + 1, 2] != 0)
                            {
                                i[a, l, 0] = 0;
                                i[a, l, 1] = 0;
                                i[a, l, 2] = 255;
                            }
                            else if (Recortada[a, l - 1, 0] != 0 || Recortada[a, l - 1, 1] != 0 || Recortada[a, l - 1, 2] != 0)
                            {
                                i[a, l, 0] = 0;
                                i[a, l, 1] = 0;
                                i[a, l, 2] = 255;
                            }
                            else if (Recortada[a - 1, l - 1, 0] != 0 || Recortada[a - 1, l - 1, 1] != 0 || Recortada[a - 1, l - 1, 2] != 0)
                            {
                                i[a, l, 0] = 0;
                                i[a, l, 1] = 0;
                                i[a, l, 2] = 255;
                            }
                            else if (Recortada[a - 1, l + 1, 0] != 0 || Recortada[a - 1, l + 1, 1] != 0 || Recortada[a - 1, l + 1, 2] != 0)
                            {
                                i[a, l, 0] = 0;
                                i[a, l, 1] = 0;
                                i[a, l, 2] = 255;
                            }
                        }
                        else if (a == 0)
                        {
                            if (Recortada[a + 1, l, 0] != 0 || Recortada[a + 1, l, 1] != 0 || Recortada[a + 1, l, 2] != 0)
                            {
                                i[a, l, 0] = 0;
                                i[a, l, 1] = 0;
                                i[a, l, 2] = 255;
                            }
                            else if (Recortada[a, l + 1, 0] != 0 || Recortada[a, l + 1, 1] != 0 || Recortada[a, l + 1, 2] != 0)
                            {
                                i[a, l, 0] = 0;
                                i[a, l, 1] = 0;
                                i[a, l, 2] = 255;
                            }
                            else if (Recortada[a, l - 1, 0] != 0 || Recortada[a, l - 1, 1] != 0 || Recortada[a, l - 1, 2] != 0)
                            {
                                i[a, l, 0] = 0;
                                i[a, l, 1] = 0;
                                i[a, l, 2] = 255;
                            }
                            else if (Recortada[a + 1, l - 1, 0] != 0 || Recortada[a + 1, l - 1, 1] != 0 || Recortada[a + 1, l - 1, 2] != 0)
                            {
                                i[a, l, 0] = 0;
                                i[a, l, 1] = 0;
                                i[a, l, 2] = 255;
                            }
                            else if (Recortada[a + 1, l + 1, 0] != 0 || Recortada[a + 1, l + 1, 1] != 0 || Recortada[a + 1, l + 1, 2] != 0)
                            {
                                i[a, l, 0] = 0;
                                i[a, l, 1] = 0;
                                i[a, l, 2] = 255;
                            }
                        }
                        else if (a != 0 && l != 0 && a != Recortada.Altura && l != Recortada.Largura)
                        {
                            if (Recortada[a + 1, l, 0] != 0 || Recortada[a + 1, l, 1] != 0 || Recortada[a + 1, l, 2] != 0)
                            {
                                i[a, l, 0] = 0;
                                i[a, l, 1] = 0;
                                i[a, l, 2] = 255;
                            }
                            else if (Recortada[a - 1, l, 0] != 0 || Recortada[a - 1, l, 1] != 0 || Recortada[a - 1, l, 2] != 0)
                            {
                                i[a, l, 0] = 0;
                                i[a, l, 1] = 0;
                                i[a, l, 2] = 255;
                            }
                            else if (Recortada[a, l + 1, 0] != 0 || Recortada[a, l + 1, 1] != 0 || Recortada[a, l + 1, 2] != 0)
                            {
                                i[a, l, 0] = 0;
                                i[a, l, 1] = 0;
                                i[a, l, 2] = 255;
                            }
                            else if (Recortada[a, l - 1, 0] != 0 || Recortada[a, l - 1, 1] != 0 || Recortada[a, l - 1, 2] != 0)
                            {
                                i[a, l, 0] = 0;
                                i[a, l, 1] = 0;
                                i[a, l, 2] = 255;
                            }
                            else if (Recortada[a + 1, l - 1, 0] != 0 || Recortada[a + 1, l - 1, 1] != 0 || Recortada[a + 1, l - 1, 2] != 0)
                            {
                                i[a, l, 0] = 0;
                                i[a, l, 1] = 0;
                                i[a, l, 2] = 255;
                            }
                            else if (Recortada[a + 1, l + 1, 0] != 0 || Recortada[a + 1, l + 1, 1] != 0 || Recortada[a + 1, l + 1, 2] != 0)
                            {
                                i[a, l, 0] = 0;
                                i[a, l, 1] = 0;
                                i[a, l, 2] = 255;
                            }
                            else if (Recortada[a - 1, l - 1, 0] != 0 || Recortada[a - 1, l - 1, 1] != 0 || Recortada[a - 1, l - 1, 2] != 0)
                            {
                                i[a, l, 0] = 0;
                                i[a, l, 1] = 0;
                                i[a, l, 2] = 255;
                            }
                            else if (Recortada[a - 1, l + 1, 0] != 0 || Recortada[a - 1, l + 1, 1] != 0 || Recortada[a - 1, l + 1, 2] != 0)
                            {
                                i[a, l, 0] = 0;
                                i[a, l, 1] = 0;
                                i[a, l, 2] = 255;
                            }
                        }
                    }
                    else
                    {
                        i[a, l, 0] = Recortada[a, l, 0];
                        i[a, l, 1] = Recortada[a, l, 1];
                        i[a, l, 2] = Recortada[a, l, 2];
                    }
                }
            }
            Recortada = i;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                Imagem = (Imagem)Abrir();
                label1.Text = "";
                pictureBox1.Image = Imagem.ToBitmap();
                Segmentar();
                Sort();
                Recorte();
                DelinearBorda();
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
            for (int i = 0; i < Segmentada.Altura; i++)
            {
                for (int j = 0; j < Segmentada.Largura; j++)
                {
                    if (Segmentada[i, j] == 255)
                    {
                        Recortada[i, j, 0] = 0;
                        Recortada[i, j, 1] = 0;
                        Recortada[i, j, 2] = 0;
                    }
                    else
                    {
                        Recortada[i, j, 0] = Imagem[i, j, 0];
                        Recortada[i, j, 1] = Imagem[i, j, 1];
                        Recortada[i, j, 2] = Imagem[i, j, 2];
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
                        modulo *= -1;
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

        private static double[] QuickSort(double[] vetor)
        {
            int inicio = 0;
            int fim = vetor.Length - 1;

            QuickSort(vetor, inicio, fim);

            return vetor;
        }

        private static void QuickSort(double[] vetor, int inicio, int fim)
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

                QuickSort(vetor, inicio, f - 1);
                QuickSort(vetor, f + 1, fim);
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
