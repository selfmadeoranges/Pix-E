using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using JPEGStructure;

namespace Pix_E
{
    public partial class Form1 : Form
    {
        Bitmap m_bitmap;
        public Form1()
        {
            InitializeComponent();
            pictureBox1.AllowDrop = true;
        }

        private void pictureBox1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files.Length > 0)
            {
                string filePath = files[0];
                m_bitmap = new Bitmap(filePath);

                JPEGFile jpeg = new JPEGFile();

                var jpegFile = jpeg.Load(filePath);

                ImageViewFrm viewFrm = new ImageViewFrm(m_bitmap);
                viewFrm.Show();
            }
        }

        private void pictureBox1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }
    }
}
