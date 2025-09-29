﻿using Dominio;
using Negocio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace WinFormsApp
{
    public partial class frmAltaArticulo : Form
    {
        private Articulo articulo;
        private List<Imagen> imagenesTemporal;
        private int indiceImagen = 0;

        public frmAltaArticulo()
        {
            InitializeComponent();
            imagenesTemporal = new List<Imagen>();
        }

        public frmAltaArticulo(Articulo articulo)
        {
            InitializeComponent();
            this.articulo = articulo;
            Text = "Modificar Artículo";

            if (articulo != null && articulo.ListaImagen != null)
            {
                imagenesTemporal = new List<Imagen>();
                foreach (var i in articulo.ListaImagen)
                {
                    imagenesTemporal.Add(new Imagen { Id = i.Id, ImagenUrl = i.ImagenUrl });
                }
            }
            else
            {
                imagenesTemporal = new List<Imagen>();
            }
        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            ArticuloNegocio negocio = new ArticuloNegocio();

            try
            {
                if (articulo == null)
                    articulo = new Articulo();

                articulo.Codigo = txtCodigo.Text;
                articulo.Nombre = txtNombre.Text;
                articulo.Descripcion = txtDescripcion.Text;
                articulo.ListaImagen = new List<Imagen>();
                
                foreach (var img in imagenesTemporal)
                {
                    if (img != null && img.ImagenUrl != null && img.ImagenUrl.Trim().Length > 0)
                    {
                        // si viene un nuevo Id (Id=0 significa nueva imagen)
                        articulo.ListaImagen.Add(new Imagen { Id = img.Id, ImagenUrl = img.ImagenUrl.Trim() });
                    }
                }

                articulo.Marca = (Marca)cboMarca.SelectedItem;
                articulo.Categoria = (Categoria)cboCategoria.SelectedItem;

                // Reemplazo las comas por ceros para tomar los decimales 
                string ingresoUsuario = txtPrecio.Text.Trim().Replace(',', '.');
                decimal precio;
                if (!decimal.TryParse(ingresoUsuario, NumberStyles.Number, CultureInfo.InvariantCulture, out precio))
                {
                    MessageBox.Show("Ingrese un precio válido.");
                    return;
                }
                articulo.Precio = precio;
                
                

                if (articulo.Id != 0)
                {
                    negocio.modificar(articulo);
                    MessageBox.Show("Modificado Exitosamente!");
                }
                else
                {
                    negocio.agregar(articulo);
                    MessageBox.Show("Agregado Exitosamente!");
                }

                Close();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString());
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void frmAltaArticulo_Load(object sender, EventArgs e)
        {
            MarcaNegocio marcaNegocio = new MarcaNegocio();
            CategoriaNegocio categoriaNegocio = new CategoriaNegocio();

            try
            {
                cboMarca.DataSource = marcaNegocio.listar();
                cboMarca.ValueMember = "Id";
                cboMarca.DisplayMember = "Descripcion";

                cboCategoria.DataSource = categoriaNegocio.listar();
                cboCategoria.ValueMember = "Id";
                cboCategoria.DisplayMember = "Descripcion";

                if (articulo != null)
                {
                    txtCodigo.Text = articulo.Codigo;
                    txtNombre.Text = articulo.Nombre;
                    txtDescripcion.Text = articulo.Descripcion;

                    imagenesTemporal = new List<Imagen>();
                    if (articulo.ListaImagen != null && articulo.ListaImagen.Count > 0)
                    {
                        foreach (var img in articulo.ListaImagen)
                        {
                            imagenesTemporal.Add(new Imagen { Id = img.Id, ImagenUrl = img.ImagenUrl });
                        }
                    }

                    if (imagenesTemporal.Count == 0)
                    {
                        imagenesTemporal.Add(new Imagen { Id = 0, ImagenUrl = "" });
                    }

                    indiceImagen = 0;
                    txtUrlImagen.Text = imagenesTemporal[indiceImagen].ImagenUrl;
                    cargarImagen(txtUrlImagen.Text);

                    cboMarca.SelectedValue= articulo.Marca.Id;
                    cboCategoria.SelectedValue = articulo.Categoria.Id;

                    txtPrecio.Text = articulo.Precio.ToString("0.00");
                }
                else
                {
                    if (imagenesTemporal.Count == 0)
                    {
                        imagenesTemporal.Add(new Imagen { Id = 0, ImagenUrl = "" });
                        indiceImagen = 0;
                    }

                    txtUrlImagen.Text = imagenesTemporal[indiceImagen].ImagenUrl;
                    cargarImagen(txtUrlImagen.Text);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            ActualizarBotonesImagen();
        }

        private void cargarImagen(string imagen)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(imagen))
                    pbxImagenArticulo.Load(imagen);
                else
                    pbxImagenArticulo.Load("https://developers.elementor.com/docs/assets/img/elementor-placeholder-image.png");
            }
            catch (Exception)
            {
                pbxImagenArticulo.Load("https://developers.elementor.com/docs/assets/img/elementor-placeholder-image.png");
            }
        }

        private void btnImagenSiguiente_Click(object sender, EventArgs e)
        {
            //GuardarURLTemporal();

            if (imagenesTemporal.Count == 0) return;

            if (indiceImagen < imagenesTemporal.Count - 1)
                indiceImagen++;

            txtUrlImagen.Text = imagenesTemporal[indiceImagen].ImagenUrl;
            cargarImagen(txtUrlImagen.Text);
            ActualizarBotonesImagen();
        }

        private void btnImagenAnterior_Click(object sender, EventArgs e)
        {
            //GuardarURLTemporal();

            if (imagenesTemporal.Count == 0) return;

            if (indiceImagen > 0)
                indiceImagen--;

            txtUrlImagen.Text = imagenesTemporal[indiceImagen].ImagenUrl;
            cargarImagen(txtUrlImagen.Text);
            ActualizarBotonesImagen();
        }

        private void txtUrlImagen_Leave(object sender, EventArgs e)
        {
            cargarImagen(txtUrlImagen.Text);
        }

        private void btnAgregarImagen_Click(object sender, EventArgs e)
        {
            //GuardarURLTemporal();

            if (indiceImagen >= 0 && indiceImagen < imagenesTemporal.Count)
                imagenesTemporal[indiceImagen].ImagenUrl = txtUrlImagen.Text.Trim();

            imagenesTemporal.Add(new Imagen { Id = 0, ImagenUrl = "" });
            indiceImagen = imagenesTemporal.Count - 1;
            txtUrlImagen.Text = "";
            cargarImagen("");

            ActualizarBotonesImagen();
        }

        private void btnEliminarImagen_Click(object sender, EventArgs e)
        {
            if (imagenesTemporal.Count == 0) return;

            int idImagenABorrar = imagenesTemporal[indiceImagen].Id;

            imagenesTemporal.RemoveAt(indiceImagen);

            if (idImagenABorrar != 0)
            {
                try
                {
                    ImagenNegocio negocioImagen = new ImagenNegocio();
                    negocioImagen.eliminar(idImagenABorrar);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al eliminar la imagen de la base de datos: " + ex.Message);
                }
            }

            if (indiceImagen >= imagenesTemporal.Count)
                indiceImagen = imagenesTemporal.Count - 1;

            if (indiceImagen >= 0)
            {
                txtUrlImagen.Text = imagenesTemporal[indiceImagen].ImagenUrl;
                cargarImagen(txtUrlImagen.Text);
            }
            else
            {
                txtUrlImagen.Text = "";
                cargarImagen("");
            }

            ActualizarBotonesImagen();
        }

        private void ActualizarBotonesImagen()
        {
            if (imagenesTemporal == null || imagenesTemporal.Count <= 1)
            {
                btnImagenAnterior.Visible = false;
                btnImagenSiguiente.Visible = false;
            }
            else
            {
                btnImagenAnterior.Visible = indiceImagen > 0;
                btnImagenSiguiente.Visible = indiceImagen < imagenesTemporal.Count - 1;
            }

            
        }

        

        
    }
}
