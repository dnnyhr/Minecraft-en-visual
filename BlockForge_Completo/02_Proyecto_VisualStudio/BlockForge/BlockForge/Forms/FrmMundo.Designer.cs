namespace BlockForge.Forms
{
    partial class FrmMundo
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.mnsPrincipal = new System.Windows.Forms.MenuStrip();
            this.mnuArchivo = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuGuardar = new System.Windows.Forms.ToolStripMenuItem();
            this.sepArchivo = new System.Windows.Forms.ToolStripSeparator();
            this.mnuSalir = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMundo = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuInventario = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuReaparecer = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuVuelo = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuVer = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuPantallaCompleta = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuDepuracion = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMultijugador = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuAbrirRed = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuChat = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuAyuda = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuControles = new System.Windows.Forms.ToolStripMenuItem();
            this.vistaMundo = new BlockForge.Controls.VistaMundo3D();
            this.stsEstado = new System.Windows.Forms.StatusStrip();
            this.lblEstadoMundo = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblEstadoPosicion = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblEstadoBloque = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblEstadoCambios = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblEstadoRed = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblAviso = new System.Windows.Forms.ToolStripStatusLabel();
            this.tmrEstado = new System.Windows.Forms.Timer(this.components);
            this.mnsPrincipal.SuspendLayout();
            this.stsEstado.SuspendLayout();
            this.SuspendLayout();
            //
            // mnsPrincipal
            //
            this.mnsPrincipal.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuArchivo,
            this.mnuMundo,
            this.mnuVer,
            this.mnuMultijugador,
            this.mnuAyuda});
            this.mnsPrincipal.BackColor = System.Drawing.Color.White;
            this.mnsPrincipal.Location = new System.Drawing.Point(0, 0);
            this.mnsPrincipal.Name = "mnsPrincipal";
            this.mnsPrincipal.Size = new System.Drawing.Size(1180, 25);
            //
            // mnuArchivo
            //
            this.mnuArchivo.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuGuardar,
            this.sepArchivo,
            this.mnuSalir});
            this.mnuArchivo.Name = "mnuArchivo";
            this.mnuArchivo.Text = "&Archivo";
            //
            // mnuGuardar
            //
            this.mnuGuardar.Name = "mnuGuardar";
            this.mnuGuardar.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.mnuGuardar.Text = "&Guardar";
            this.mnuGuardar.Click += new System.EventHandler(this.mnuGuardar_Click);
            //
            // sepArchivo
            //
            this.sepArchivo.Name = "sepArchivo";
            //
            // mnuSalir
            //
            this.mnuSalir.Name = "mnuSalir";
            this.mnuSalir.Text = "&Salir al menú";
            this.mnuSalir.Click += new System.EventHandler(this.mnuSalir_Click);
            //
            // mnuMundo
            //
            this.mnuMundo.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuInventario,
            this.mnuReaparecer,
            this.mnuVuelo});
            this.mnuMundo.Name = "mnuMundo";
            this.mnuMundo.Text = "&Mundo";
            //
            // mnuInventario
            //
            this.mnuInventario.Name = "mnuInventario";
            this.mnuInventario.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.I)));
            this.mnuInventario.Text = "&Inventario";
            this.mnuInventario.Click += new System.EventHandler(this.mnuInventario_Click);
            //
            // mnuReaparecer
            //
            this.mnuReaparecer.Name = "mnuReaparecer";
            this.mnuReaparecer.ShortcutKeyDisplayString = "R";
            this.mnuReaparecer.Text = "Volver al punto de &aparición";
            this.mnuReaparecer.Click += new System.EventHandler(this.mnuReaparecer_Click);
            //
            // mnuVuelo
            //
            this.mnuVuelo.Name = "mnuVuelo";
            this.mnuVuelo.ShortcutKeyDisplayString = "F";
            this.mnuVuelo.Text = "Modo &vuelo";
            this.mnuVuelo.Click += new System.EventHandler(this.mnuVuelo_Click);
            //
            // mnuVer
            //
            this.mnuVer.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuPantallaCompleta,
            this.mnuDepuracion});
            this.mnuVer.Name = "mnuVer";
            this.mnuVer.Text = "&Ver";
            //
            // mnuPantallaCompleta
            //
            this.mnuPantallaCompleta.Name = "mnuPantallaCompleta";
            this.mnuPantallaCompleta.ShortcutKeys = System.Windows.Forms.Keys.F11;
            this.mnuPantallaCompleta.Text = "&Pantalla completa";
            this.mnuPantallaCompleta.Click += new System.EventHandler(this.mnuPantallaCompleta_Click);
            //
            // mnuDepuracion
            //
            this.mnuDepuracion.CheckOnClick = true;
            this.mnuDepuracion.Name = "mnuDepuracion";
            this.mnuDepuracion.ShortcutKeys = System.Windows.Forms.Keys.F3;
            this.mnuDepuracion.Text = "&Información de depuración";
            this.mnuDepuracion.Click += new System.EventHandler(this.mnuDepuracion_Click);
            //
            // mnuMultijugador
            //
            this.mnuMultijugador.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuAbrirRed,
            this.mnuChat});
            this.mnuMultijugador.Name = "mnuMultijugador";
            this.mnuMultijugador.Text = "Multi&jugador";
            //
            // mnuAbrirRed
            //
            this.mnuAbrirRed.Name = "mnuAbrirRed";
            this.mnuAbrirRed.Text = "&Abrir a la red (local o internet)...";
            this.mnuAbrirRed.Click += new System.EventHandler(this.mnuAbrirRed_Click);
            //
            // mnuChat
            //
            this.mnuChat.Name = "mnuChat";
            this.mnuChat.ShortcutKeyDisplayString = "T";
            this.mnuChat.Text = "&Chat";
            this.mnuChat.Click += new System.EventHandler(this.mnuChat_Click);
            //
            // mnuAyuda
            //
            this.mnuAyuda.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuControles});
            this.mnuAyuda.Name = "mnuAyuda";
            this.mnuAyuda.Text = "A&yuda";
            //
            // mnuControles
            //
            this.mnuControles.Name = "mnuControles";
            this.mnuControles.ShortcutKeys = System.Windows.Forms.Keys.F1;
            this.mnuControles.Text = "&Controles";
            this.mnuControles.Click += new System.EventHandler(this.mnuControles_Click);
            //
            // vistaMundo
            //
            this.vistaMundo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.vistaMundo.Location = new System.Drawing.Point(0, 25);
            this.vistaMundo.Name = "vistaMundo";
            this.vistaMundo.Size = new System.Drawing.Size(1180, 629);
            this.vistaMundo.TabIndex = 0;
            this.vistaMundo.EstadoCambiado += new System.EventHandler(this.vistaMundo_EstadoCambiado);
            this.vistaMundo.AccionSolicitada += new System.EventHandler<BlockForge.Controls.AccionEventArgs>(this.vistaMundo_AccionSolicitada);
            //
            // stsEstado
            //
            this.stsEstado.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblEstadoMundo,
            this.lblEstadoPosicion,
            this.lblEstadoBloque,
            this.lblEstadoCambios,
            this.lblEstadoRed,
            this.lblAviso});
            this.stsEstado.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(230)))), ((int)(((byte)(220)))));
            this.stsEstado.Location = new System.Drawing.Point(0, 654);
            this.stsEstado.Name = "stsEstado";
            this.stsEstado.Size = new System.Drawing.Size(1180, 26);
            //
            // lblEstadoMundo
            //
            this.lblEstadoMundo.Name = "lblEstadoMundo";
            this.lblEstadoMundo.Text = "Mundo:";
            //
            // lblEstadoPosicion
            //
            this.lblEstadoPosicion.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this.lblEstadoPosicion.Name = "lblEstadoPosicion";
            this.lblEstadoPosicion.Text = "Posición: -";
            //
            // lblEstadoBloque
            //
            this.lblEstadoBloque.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this.lblEstadoBloque.Name = "lblEstadoBloque";
            this.lblEstadoBloque.Text = "Bloque activo: -";
            //
            // lblEstadoCambios
            //
            this.lblEstadoCambios.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this.lblEstadoCambios.Name = "lblEstadoCambios";
            this.lblEstadoCambios.Text = "Sin cambios";
            //
            // lblEstadoRed
            //
            this.lblEstadoRed.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this.lblEstadoRed.Name = "lblEstadoRed";
            this.lblEstadoRed.Text = "Un jugador";
            //
            // lblAviso
            //
            this.lblAviso.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(99)))), ((int)(((byte)(104)))));
            this.lblAviso.Name = "lblAviso";
            this.lblAviso.Spring = true;
            this.lblAviso.Text = "Esc libera el mouse · F1 muestra los controles";
            this.lblAviso.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // tmrEstado
            //
            this.tmrEstado.Enabled = true;
            this.tmrEstado.Interval = 250;
            this.tmrEstado.Tick += new System.EventHandler(this.tmrEstado_Tick);
            //
            // FrmMundo
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(1180, 680);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.MainMenuStrip = this.mnsPrincipal;
            this.MinimumSize = new System.Drawing.Size(800, 520);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "BlockForge";
            this.Controls.Add(this.vistaMundo);
            this.Controls.Add(this.stsEstado);
            this.Controls.Add(this.mnsPrincipal);
            this.Name = "FrmMundo";
            this.Load += new System.EventHandler(this.FrmMundo_Load);
            this.Shown += new System.EventHandler(this.FrmMundo_Shown);
            this.Deactivate += new System.EventHandler(this.FrmMundo_Deactivate);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmMundo_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FrmMundo_FormClosed);
            this.mnsPrincipal.ResumeLayout(false);
            this.mnsPrincipal.PerformLayout();
            this.stsEstado.ResumeLayout(false);
            this.stsEstado.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.MenuStrip mnsPrincipal;
        private System.Windows.Forms.ToolStripMenuItem mnuArchivo;
        private System.Windows.Forms.ToolStripMenuItem mnuGuardar;
        private System.Windows.Forms.ToolStripSeparator sepArchivo;
        private System.Windows.Forms.ToolStripMenuItem mnuSalir;
        private System.Windows.Forms.ToolStripMenuItem mnuMundo;
        private System.Windows.Forms.ToolStripMenuItem mnuInventario;
        private System.Windows.Forms.ToolStripMenuItem mnuReaparecer;
        private System.Windows.Forms.ToolStripMenuItem mnuVuelo;
        private System.Windows.Forms.ToolStripMenuItem mnuVer;
        private System.Windows.Forms.ToolStripMenuItem mnuPantallaCompleta;
        private System.Windows.Forms.ToolStripMenuItem mnuDepuracion;
        private System.Windows.Forms.ToolStripMenuItem mnuMultijugador;
        private System.Windows.Forms.ToolStripMenuItem mnuAbrirRed;
        private System.Windows.Forms.ToolStripMenuItem mnuChat;
        private System.Windows.Forms.ToolStripMenuItem mnuAyuda;
        private System.Windows.Forms.ToolStripMenuItem mnuControles;
        private BlockForge.Controls.VistaMundo3D vistaMundo;
        private System.Windows.Forms.StatusStrip stsEstado;
        private System.Windows.Forms.ToolStripStatusLabel lblEstadoMundo;
        private System.Windows.Forms.ToolStripStatusLabel lblEstadoPosicion;
        private System.Windows.Forms.ToolStripStatusLabel lblEstadoBloque;
        private System.Windows.Forms.ToolStripStatusLabel lblEstadoCambios;
        private System.Windows.Forms.ToolStripStatusLabel lblEstadoRed;
        private System.Windows.Forms.ToolStripStatusLabel lblAviso;
        private System.Windows.Forms.Timer tmrEstado;
    }
}
