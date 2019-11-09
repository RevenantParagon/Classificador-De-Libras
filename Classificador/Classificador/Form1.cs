﻿using System;
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
        private Imagem Binaria;

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
                MascaraLimiar();
            }
            catch (Exception)
            {
                MessageBox.Show("Houve um erro na seleção de imagem", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

        private void MascaraLimiar()
        {
            Imagem cinza = Normalizada.ToGrayscale();
            Binaria = new Imagem(Imagem.Largura, Imagem.Altura, TipoImagem.Monocromatica);

            for (int a = 0; a < Imagem.Altura; a++)
            {
                for (int l = 0; l < Imagem.Largura; l++)
                {
                    if (cinza[a, l] <= 85)
                    {
                        Binaria[a, l] = 0;
                    }
                    else
                    {
                        Binaria[a, l] = 255;
                    }
                }
            }
            pictureBox3.Image = Binaria.ToBitmap();
        }

    }
}
