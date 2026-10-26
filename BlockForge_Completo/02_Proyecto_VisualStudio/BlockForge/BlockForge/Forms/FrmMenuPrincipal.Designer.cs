namespace BlockForge.Forms
{
    partial class FrmMenuPrincipal
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
            this.pnlEncabezado = new System.Windows.Forms.Panel();
            this.lblUsuario = new System.Windows.Forms.Label();
            this.btnCerrarSesion = new System.Windows.Forms.Button();
            this.btnSkin = new System.Windows.Forms.Button();
            this.lblJugar = new System.Windows.Forms.Label();
            this.btnNuevoMundo = new BlockForge.Controls.TarjetaMenu();
            this.btnCargarMundo = new BlockForge.Controls.TarjetaMenu();
            this.lblMultijugador = new System.Windows.Forms.Label();
            this.btnHospedar = new BlockForge.Controls.TarjetaMenu();
            this.btnUnirse = new BlockForge.Controls.TarjetaMenu();
            this.lblAdministracion = new System.Windows.Forms.Label();
            this.btnGestionPartidas = new BlockForge.Controls.TarjetaMenu();
            this.btnConfiguracion = new BlockForge.Controls.TarjetaMenu();
            this.lblNotaAdmin = new System.Windows.Forms.Label();
            this.stsEstado = new System.Windows.Forms.StatusStrip();
            this.lblEstado = new System.Windows.Forms.ToolStripStatusLabel();
            this.pnlEncabezado.SuspendLayout();
            this.stsEstado.SuspendLayout();
            this.SuspendLayout();
            //
            // pnlEncabezado
            //
            this.pnlEncabezado.Controls.Add(this.lblUsuario);
            this.pnlEncabezado.Controls.Add(this.btnSkin);
            this.pnlEncabezado.Controls.Add(this.btnCerrarSesion);
            this.pnlEncabezado.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(92)))), ((int)(((byte)(152)))), ((int)(((byte)(240)))));
            this.pnlEncabezado.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlEncabezado.Location = new System.Drawing.Point(0, 0);
            this.pnlEncabezado.Name = "pnlEncabezado";
            this.pnlEncabezado.Size = new System.Drawing.Size(820, 150);
            this.pnlEncabezado.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlEncabezado_Paint);
            //
            // lblUsuario
            //
            this.lblUsuario.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.lblUsuario.BackColor = System.Drawing.Color.Transparent;
            this.lblUsuario.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblUsuario.ForeColor = System.Drawing.Color.White;
            this.lblUsuario.Location = new System.Drawing.Point(300, 16);
            this.lblUsuario.Name = "lblUsuario";
            this.lblUsuario.Size = new System.Drawing.Size(240, 40);
            this.lblUsuario.Text = "Usuario\r\nRol";
            this.lblUsuario.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            //
            // btnCerrarSesion
            //
            this.btnCerrarSesion.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnCerrarSesion.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(45)))), ((int)(((byte)(50)))));
            this.btnCerrarSesion.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCerrarSesion.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(205)))), ((int)(((byte)(215)))));
            this.btnCerrarSesion.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(65)))), ((int)(((byte)(72)))));
            this.btnCerrarSesion.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCerrarSesion.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnCerrarSesion.ForeColor = System.Drawing.Color.White;
            this.btnCerrarSesion.Location = new System.Drawing.Point(684, 18);
            this.btnCerrarSesion.Name = "btnCerrarSesion";
            this.btnCerrarSesion.Size = new System.Drawing.Size(120, 36);
            this.btnCerrarSesion.TabIndex = 7;
            this.btnCerrarSesion.Text = "Cerrar sesión";
            this.btnCerrarSesion.UseVisualStyleBackColor = false;
            this.btnCerrarSesion.Click += new System.EventHandler(this.btnCerrarSesion_Click);
            //
            // btnSkin
            //
            this.btnSkin.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnSkin.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(45)))), ((int)(((byte)(50)))));
            this.btnSkin.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSkin.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(205)))), ((int)(((byte)(215)))));
            this.btnSkin.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(65)))), ((int)(((byte)(72)))));
            this.btnSkin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSkin.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnSkin.ForeColor = System.Drawing.Color.White;
            this.btnSkin.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSkin.Location = new System.Drawing.Point(552, 18);
            this.btnSkin.Name = "btnSkin";
            this.btnSkin.Padding = new System.Windows.Forms.Padding(6, 0, 4, 0);
            this.btnSkin.Size = new System.Drawing.Size(124, 36);
            this.btnSkin.TabIndex = 6;
            this.btnSkin.Text = "Mi skin";
            this.btnSkin.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnSkin.UseVisualStyleBackColor = false;
            this.btnSkin.Click += new System.EventHandler(this.btnSkin_Click);
            //
            // lblJugar
            //
            this.lblJugar.AutoSize = true;
            this.lblJugar.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblJugar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(181)))), ((int)(((byte)(186)))), ((int)(((byte)(193)))));
            this.lblJugar.Location = new System.Drawing.Point(24, 166);
            this.lblJugar.Name = "lblJugar";
            this.lblJugar.Text = "UN JUGADOR";
            //
            // btnNuevoMundo
            //
            this.btnNuevoMundo.Acento = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(175)))), ((int)(((byte)(80)))));
            this.btnNuevoMundo.Bloque = "Pasto";
            this.btnNuevoMundo.Descripcion = "Genera un terreno 3D con colinas, lagos, playas y árboles.";
            this.btnNuevoMundo.Location = new System.Drawing.Point(26, 196);
            this.btnNuevoMundo.Name = "btnNuevoMundo";
            this.btnNuevoMundo.Size = new System.Drawing.Size(378, 78);
            this.btnNuevoMundo.TabIndex = 0;
            this.btnNuevoMundo.Text = "Nuevo mundo";
            this.btnNuevoMundo.Click += new System.EventHandler(this.btnNuevoMundo_Click);
            //
            // btnCargarMundo
            //
            this.btnCargarMundo.Acento = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(175)))), ((int)(((byte)(80)))));
            this.btnCargarMundo.Bloque = "Madera";
            this.btnCargarMundo.Descripcion = "Continúa uno de tus mundos guardados.";
            this.btnCargarMundo.Location = new System.Drawing.Point(416, 196);
            this.btnCargarMundo.Name = "btnCargarMundo";
            this.btnCargarMundo.Size = new System.Drawing.Size(378, 78);
            this.btnCargarMundo.TabIndex = 1;
            this.btnCargarMundo.Text = "Cargar mundo";
            this.btnCargarMundo.Click += new System.EventHandler(this.btnCargarMundo_Click);
            //
            // lblMultijugador
            //
            this.lblMultijugador.AutoSize = true;
            this.lblMultijugador.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblMultijugador.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(181)))), ((int)(((byte)(186)))), ((int)(((byte)(193)))));
            this.lblMultijugador.Location = new System.Drawing.Point(24, 292);
            this.lblMultijugador.Name = "lblMultijugador";
            this.lblMultijugador.Text = "MULTIJUGADOR (RED LOCAL E INTERNET)";
            //
            // btnHospedar
            //
            this.btnHospedar.Acento = System.Drawing.Color.FromArgb(((int)(((byte)(66)))), ((int)(((byte)(133)))), ((int)(((byte)(244)))));
            this.btnHospedar.Bloque = "Tronco";
            this.btnHospedar.Descripcion = "Abre uno de tus mundos a tu red local o a internet para jugar con amigos.";
            this.btnHospedar.Location = new System.Drawing.Point(26, 322);
            this.btnHospedar.Name = "btnHospedar";
            this.btnHospedar.Size = new System.Drawing.Size(378, 78);
            this.btnHospedar.TabIndex = 2;
            this.btnHospedar.Text = "Hospedar partida";
            this.btnHospedar.Click += new System.EventHandler(this.btnHospedar_Click);
            //
            // btnUnirse
            //
            this.btnUnirse.Acento = System.Drawing.Color.FromArgb(((int)(((byte)(66)))), ((int)(((byte)(133)))), ((int)(((byte)(244)))));
            this.btnUnirse.Bloque = "Arena";
            this.btnUnirse.Descripcion = "Busca partidas en tu red o pega la invitación (IP:puerto) de un amigo.";
            this.btnUnirse.Location = new System.Drawing.Point(416, 322);
            this.btnUnirse.Name = "btnUnirse";
            this.btnUnirse.Size = new System.Drawing.Size(378, 78);
            this.btnUnirse.TabIndex = 3;
            this.btnUnirse.Text = "Unirse a partida";
            this.btnUnirse.Click += new System.EventHandler(this.btnUnirse_Click);
            //
            // lblAdministracion
            //
            this.lblAdministracion.AutoSize = true;
            this.lblAdministracion.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblAdministracion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(181)))), ((int)(((byte)(186)))), ((int)(((byte)(193)))));
            this.lblAdministracion.Location = new System.Drawing.Point(24, 418);
            this.lblAdministracion.Name = "lblAdministracion";
            this.lblAdministracion.Text = "ADMINISTRACIÓN";
            //
            // btnGestionPartidas
            //
            this.btnGestionPartidas.Acento = System.Drawing.Color.FromArgb(((int)(((byte)(201)))), ((int)(((byte)(137)))), ((int)(((byte)(72)))));
            this.btnGestionPartidas.Bloque = "Grava";
            this.btnGestionPartidas.Descripcion = "Consulta, restablece o elimina las partidas guardadas.";
            this.btnGestionPartidas.Location = new System.Drawing.Point(26, 448);
            this.btnGestionPartidas.Name = "btnGestionPartidas";
            this.btnGestionPartidas.Size = new System.Drawing.Size(378, 78);
            this.btnGestionPartidas.TabIndex = 4;
            this.btnGestionPartidas.Text = "Gestión de partidas";
            this.btnGestionPartidas.Click += new System.EventHandler(this.btnGestionPartidas_Click);
            //
            // btnConfiguracion
            //
            this.btnConfiguracion.Acento = System.Drawing.Color.FromArgb(((int)(((byte)(201)))), ((int)(((byte)(137)))), ((int)(((byte)(72)))));
            this.btnConfiguracion.Bloque = "Piedra";
            this.btnConfiguracion.Descripcion = "Ajusta la distancia de visión, el mouse y los valores predeterminados.";
            this.btnConfiguracion.Location = new System.Drawing.Point(416, 448);
            this.btnConfiguracion.Name = "btnConfiguracion";
            this.btnConfiguracion.Size = new System.Drawing.Size(378, 78);
            this.btnConfiguracion.TabIndex = 5;
            this.btnConfiguracion.Text = "Configuración";
            this.btnConfiguracion.Click += new System.EventHandler(this.btnConfiguracion_Click);
            //
            // lblNotaAdmin
            //
            this.lblNotaAdmin.AutoSize = true;
            this.lblNotaAdmin.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblNotaAdmin.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(130)))), ((int)(((byte)(135)))), ((int)(((byte)(145)))));
            this.lblNotaAdmin.Location = new System.Drawing.Point(26, 536);
            this.lblNotaAdmin.Name = "lblNotaAdmin";
            this.lblNotaAdmin.Text = "La sección de Administración solo se muestra a usuarios con rol Administrador.";
            //
            // stsEstado
            //
            this.stsEstado.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(25)))), ((int)(((byte)(28)))));
            this.stsEstado.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblEstado});
            this.stsEstado.Location = new System.Drawing.Point(0, 570);
            this.stsEstado.Name = "stsEstado";
            this.stsEstado.Size = new System.Drawing.Size(820, 26);
            this.stsEstado.SizingGrip = false;
            //
            // lblEstado
            //
            this.lblEstado.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(181)))), ((int)(((byte)(186)))), ((int)(((byte)(193)))));
            this.lblEstado.Name = "lblEstado";
            this.lblEstado.Text = "Listo";
            //
            // FrmMenuPrincipal
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(31)))), ((int)(((byte)(34)))));
            this.ClientSize = new System.Drawing.Size(820, 596);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(232)))), ((int)(((byte)(235)))));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = true;
            this.ShowInTaskbar = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "BlockForge — Menú principal";
            this.Controls.Add(this.pnlEncabezado);
            this.Controls.Add(this.lblJugar);
            this.Controls.Add(this.btnNuevoMundo);
            this.Controls.Add(this.btnCargarMundo);
            this.Controls.Add(this.lblMultijugador);
            this.Controls.Add(this.btnHospedar);
            this.Controls.Add(this.btnUnirse);
            this.Controls.Add(this.lblAdministracion);
            this.Controls.Add(this.btnGestionPartidas);
            this.Controls.Add(this.btnConfiguracion);
            this.Controls.Add(this.lblNotaAdmin);
            this.Controls.Add(this.stsEstado);
            this.Name = "FrmMenuPrincipal";
            this.Load += new System.EventHandler(this.FrmMenuPrincipal_Load);
            this.pnlEncabezado.ResumeLayout(false);
            this.stsEstado.ResumeLayout(false);
            this.stsEstado.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Panel pnlEncabezado;
        private System.Windows.Forms.Label lblUsuario;
        private System.Windows.Forms.Button btnCerrarSesion;
        private System.Windows.Forms.Button btnSkin;
        private System.Windows.Forms.Label lblJugar;
        private BlockForge.Controls.TarjetaMenu btnNuevoMundo;
        private BlockForge.Controls.TarjetaMenu btnCargarMundo;
        private System.Windows.Forms.Label lblMultijugador;
        private BlockForge.Controls.TarjetaMenu btnHospedar;
        private BlockForge.Controls.TarjetaMenu btnUnirse;
        private System.Windows.Forms.Label lblAdministracion;
        private BlockForge.Controls.TarjetaMenu btnGestionPartidas;
        private BlockForge.Controls.TarjetaMenu btnConfiguracion;
        private System.Windows.Forms.Label lblNotaAdmin;
        private System.Windows.Forms.StatusStrip stsEstado;
        private System.Windows.Forms.ToolStripStatusLabel lblEstado;
    }
}
