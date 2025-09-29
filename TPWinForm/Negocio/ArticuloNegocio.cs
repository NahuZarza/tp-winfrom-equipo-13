﻿using Dominio;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio
{
    public class ArticuloNegocio
    {
        public List<Articulo> listar()
        {
            List<Articulo> lista = new List<Articulo>();
            ImagenNegocio imagenNegocio = new ImagenNegocio();
            AccesoDatos datos = new AccesoDatos();

            try
            {
                datos.setearConsulta("SELECT A.Id, Codigo, Nombre, A.Descripcion, M.Descripcion Marca, " +
                                     "C.Descripcion Categoria, A.IdMarca, A.IdCategoria, " +
                                     "Precio FROM ARTICULOS A, MARCAS M, CATEGORIAS C " +
                                     "WHERE M.Id = A.IdMarca AND C.Id = A.IdCategoria");
                datos.ejecutarLectura();

                while (datos.Lector.Read())
                {
                    Articulo aux = new Articulo();
                    aux.Id = (int)datos.Lector["Id"];
                    aux.Codigo = (string)datos.Lector["Codigo"];
                    aux.Nombre = (string)datos.Lector["Nombre"];
                    aux.Descripcion = (string)datos.Lector["Descripcion"];

                    aux.Marca = new Marca();
                    aux.Marca.Id = (int)datos.Lector["IdMarca"];
                    aux.Marca.Descripcion = (string)datos.Lector["Marca"];

                    aux.Categoria = new Categoria();
                    aux.Categoria.Id = (int)datos.Lector["IdCategoria"];
                    aux.Categoria.Descripcion = (string)datos.Lector["Categoria"];

                    aux.Precio = (decimal)datos.Lector["Precio"];

                    // ES OPCIONAL: rendondeo los decimales a 2 nums despues de la coma
                    aux.Precio = Math.Round(aux.Precio, 2, MidpointRounding.AwayFromZero);

                    //Agregar lista de imagenes
                    List<Imagen> listaImagen = new List<Imagen>();
                    aux.ListaImagen = imagenNegocio.listarPorIdArticulo(aux.Id);
                    lista.Add(aux);
                }
                return lista;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public void agregar(Articulo nuevo)
        {
            AccesoDatos datos = new AccesoDatos();
            AccesoDatos datosImagen = new AccesoDatos();

            try
            {
                datos.setearConsulta("INSERT INTO ARTICULOS (Codigo, Nombre, Descripcion, " +
                                     "IdMarca, IdCategoria, Precio) " +
                                     "VALUES ('" + nuevo.Codigo + "', '" + nuevo.Nombre + "', '" 
                                     + nuevo.Descripcion + "', @idMarca, @idCategoria, @precio)");
                datos.setearParametro("@idMarca", nuevo.Marca.Id);
                datos.setearParametro("@idCategoria", nuevo.Categoria.Id);
                datos.setearParametro("@precio", nuevo.Precio);
                
                datos.ejecutarAccion();
                datos.cerrarConexion();

                int idArticulo = 0;
                datos.setearConsulta("SELECT Id FROM ARTICULOS WHERE Codigo = '" + nuevo.Codigo + "'");
                datos.ejecutarLectura();

                if (datos.Lector.Read())
                {
                    idArticulo = (int)datos.Lector["Id"];
                }
                datos.cerrarConexion();

                if (nuevo.ListaImagen != null && nuevo.ListaImagen.Count > 0)
                {
                    foreach (var imagen in nuevo.ListaImagen)
                    {
                        datosImagen = new AccesoDatos();
                        datosImagen.setearConsulta("INSERT INTO Imagenes (IdArticulo, ImagenUrl) " +
                                                   "VALUES (@idArticulo, @imagenUrl)");
                        datosImagen.setearParametro("@idArticulo", idArticulo);
                        datosImagen.setearParametro("@imagenUrl", imagen.ImagenUrl);
                        datosImagen.ejecutarAccion();
                        datosImagen.cerrarConexion();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                datos.cerrarConexion();
                datosImagen.cerrarConexion();
            }
        }

        public void modificar(Articulo existente)
        {
            AccesoDatos datos = new AccesoDatos();
            AccesoDatos datosImagen = new AccesoDatos();

            try
            {
                datos.setearConsulta("UPDATE ARTICULOS SET Codigo = @codigo, Nombre = @nombre, " +
                                     "Descripcion = @descripcion, IdMarca = @idmarca, " +
                                     "IdCategoria = @idcategoria, Precio = @precio WHERE Id = @id");
                datos.setearParametro("@codigo", existente.Codigo);
                datos.setearParametro("@nombre", existente.Nombre);
                datos.setearParametro("@descripcion", existente.Descripcion);
                datos.setearParametro("@idmarca", existente.Marca.Id);
                datos.setearParametro("@idcategoria", existente.Categoria.Id);
                datos.setearParametro("@precio", existente.Precio);
                datos.setearParametro("@id", existente.Id);
                datos.ejecutarAccion();
                datos.cerrarConexion();

                List<int> idsActuales = new List<int>();
                datosImagen = new AccesoDatos();

                datosImagen.setearConsulta("SELECT Id FROM IMAGENES WHERE IdArticulo = @idArticulo");
                datosImagen.setearParametro("@idArticulo", existente.Id);
                datosImagen.ejecutarLectura();

                while (datosImagen.Lector.Read())
                {
                    idsActuales.Add((int)datosImagen.Lector["Id"]);
                }
                datosImagen.cerrarConexion();

                
                //lista de los IDS que se quieren conservar
                List<int> idsConservados = new List<int>();
                foreach (var imagen in existente.ListaImagen)
                {
                    if (imagen.Id != 0)
                        idsConservados.Add(imagen.Id);
                }

                // Elimino las que ya no se usan
                foreach (int idBD in idsActuales)
                {
                    if (!idsConservados.Contains(idBD))
                    {
                        datosImagen = new AccesoDatos();
                        datosImagen.setearConsulta("DELETE FROM IMAGENES WHERE Id = @idImagen");
                        datosImagen.setearParametro("@idImagen", idBD);
                        datosImagen.ejecutarAccion();
                        datosImagen.cerrarConexion();
                    }
                }

                // INSERT / UPDATE de las imagenes 
                if (existente.ListaImagen != null && existente.ListaImagen.Count > 0)
                {
                    foreach (var imagen in existente.ListaImagen)
                    {
                        datosImagen = new AccesoDatos();

                        if (imagen.Id != 0)
                        {
                            // UPDATE imagen existente
                            datosImagen.setearConsulta("UPDATE IMAGENES SET ImagenUrl = @imagenUrl WHERE Id = @idImagen");
                            datosImagen.setearParametro("@idImagen", imagen.Id);
                            datosImagen.setearParametro("@imagenUrl", imagen.ImagenUrl);
                        }
                        else
                        {
                            // INSERT nueva imagen
                            datosImagen.setearConsulta("INSERT INTO IMAGENES (IdArticulo, ImagenUrl) VALUES (@idArticulo, @imagenUrl)");
                            datosImagen.setearParametro("@idArticulo", existente.Id);
                            datosImagen.setearParametro("@imagenUrl", imagen.ImagenUrl);
                        }

                        datosImagen.ejecutarAccion();
                        datosImagen.cerrarConexion();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                datos.cerrarConexion();
                datosImagen.cerrarConexion();
            }
        }

        public void eliminar(int id)
        {
            AccesoDatos datos = new AccesoDatos();
            AccesoDatos datosImagen = new AccesoDatos();
            try
            {
                datos.setearConsulta("DELETE FROM ARTICULOS WHERE Id = @id");
                datos.setearParametro("@id", id);
                datos.ejecutarAccion();
                datos.cerrarConexion();

                datosImagen.setearConsulta("DELETE FROM IMAGENES WHERE IdArticulo = @id");
                datosImagen.setearParametro("@id", id);
                datosImagen.ejecutarAccion();
                datosImagen.cerrarConexion();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                datos.cerrarConexion();
                datosImagen.cerrarConexion();
            }
        }
    }
}
