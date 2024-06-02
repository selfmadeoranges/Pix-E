using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pix_E
{
    public partial class ImageViewFrm : Form
    {
        Bitmap m_bitmap;
        public ImageViewFrm(Bitmap bitmap)
        {
            InitializeComponent();
            m_bitmap = bitmap;
        }

        private void ImageViewFrm_Load(object sender, EventArgs e)
        {
            pictureBox1.Image = m_bitmap;
        }
    }
}
