﻿/*
	Copyright (c) 2016 Batuhan KINDAN

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tao.OpenGl;
using Tao.Platform.Windows;
using System.IO;
using System.Globalization;
using System.Threading;

namespace triangle_mesh_1
{
    public partial class Form1 : Form
    {
        //-------------------------------------------------------------------
        int numberOfCoords = 0;
        double[] vaoTriangle; // triangle array for openGl vertex array object
        double[] vaoNormal; // normal array for openGl vertex array object
        List<double> normal = new List<double>(); // normal coordinates list for defining the proper lenght of normal array
        List<double> triangle = new List<double>(); // triangle coordinates list for defining the proper lenght of triangle array
        //-------------------------------------------------------------------        
        int rotX = 0;
        int rotY = 0;
        int rotZ = 0;
        double scale = 1.5;
        double tX = 0;
        double tY = 0;
        //-------------------------------------------------------------------
        bool keyW = false;
        bool keyA = false;
        bool keyS = false;
        bool keyD = false;
        bool keyQ = false;
        bool keyE = false;
        bool keyAdd = false;
        bool keySub = false;
        //-------------------------------------------------------------------

        public Form1()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            InitializeComponent();
            glMonitor.InitializeContexts();
            glInit(glMonitor);
        }
        
        void glInit(SimpleOpenGlControl formOpenGlMonitor)
        {
            Gl.glClearColor(0, 0, 0, 0);
            Gl.glLoadIdentity();
            Gl.glViewport(0, 0, formOpenGlMonitor.Width, formOpenGlMonitor.Height);
            Gl.glOrtho(-formOpenGlMonitor.Width / 2, formOpenGlMonitor.Width / 2, -formOpenGlMonitor.Height / 2, formOpenGlMonitor.Height / 2, 2000, -2000); // origin defined
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
            Gl.glClearDepth(1.0f);
            Gl.glEnable(Gl.GL_DEPTH_TEST);
            Gl.glDepthFunc(Gl.GL_LEQUAL);
            Gl.glHint(Gl.GL_PERSPECTIVE_CORRECTION_HINT, Gl.GL_NICEST);
            Gl.glEnable(Gl.GL_BLEND);
            Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA); 
            //-------------------------------------------------------------------
            float[] shine = new float[] { 0.25f, 0.25f, 0.25f, 1.0f };
            float[] shine2 = new float[] { 0.35f, 0.35f, 0.35f, 1.0f };
            float[] specref = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };
            float[] specular = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };
            float[] lightPos = new float[] { 300f, 300f, -200.0f, 1.0f };  
            float[] lightPos2 = new float[] { -300f, 300f, -200.0f, 1.0f }; 
            //-------------------------------------------------------------------
            Gl.glEnable(Gl.GL_LIGHTING);  // Lighting enabled  
            //-------------------------------------------------------------------
            //--Light 0                              
            Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_AMBIENT, shine);
            Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_DIFFUSE, shine2); 
            Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_SPECULAR, specular); 
            Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_POSITION, lightPos); 
            Gl.glEnable(Gl.GL_LIGHT0); // Light 0 enabled
            //------------------------------------------------------------------
            //--Light 1
            Gl.glLightfv(Gl.GL_LIGHT1, Gl.GL_AMBIENT, shine);
            Gl.glLightfv(Gl.GL_LIGHT1, Gl.GL_DIFFUSE, shine2);
            Gl.glLightfv(Gl.GL_LIGHT1, Gl.GL_SPECULAR, specular);
            Gl.glLightfv(Gl.GL_LIGHT1, Gl.GL_POSITION, lightPos2);
            Gl.glEnable(Gl.GL_LIGHT1);
            //-----------------------------------------------------------------
            Gl.glEnable(Gl.GL_COLOR_MATERIAL); 
            Gl.glColorMaterial(Gl.GL_FRONT, Gl.GL_AMBIENT_AND_DIFFUSE);
            Gl.glMaterialfv(Gl.GL_FRONT, Gl.GL_SPECULAR, specref);
            Gl.glMateriali(Gl.GL_FRONT, Gl.GL_SHININESS, 2); // material shine feature defined
            Gl.glEnable(Gl.GL_NORMALIZE); 
        }

        void glDynamic(SimpleOpenGlControl formOpenGlMonitor)
        {
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glLoadIdentity();
            Gl.glViewport(0, 0, formOpenGlMonitor.Width, formOpenGlMonitor.Height); // change gl secreen viewport dynamicly
            Gl.glOrtho(-formOpenGlMonitor.Width / 2, formOpenGlMonitor.Width / 2, -formOpenGlMonitor.Height / 2, formOpenGlMonitor.Height / 2, 2000, -2000); // origin redefined
        }

        public void readStl(string stlFilePath)
        {
            string stlLine;
            int i = 0; // sentinel

            StreamReader stlReader = new StreamReader(stlFilePath);
            while (!stlReader.EndOfStream) 
            {
                stlLine = stlReader.ReadLine().Trim().Replace(" ", "");

                switch (stlLine)
                {
                    case "solid":

                        while (stlLine != "endsolid")
                        {
                            stlLine = stlReader.ReadLine().Trim().Replace(" ", ""); //facetNormal

                            if (stlLine == "endsolid") // Chacking endsolid line 
                            {
                                break;
                            }

                            // FaceNormal     
                            normal.Add(double.Parse(stlLine.Substring(11, 14))); // x1
                            normal.Add(double.Parse(stlLine.Substring(25, 14))); // y1
                            normal.Add(double.Parse(stlLine.Substring(39, 14))); // z1
                            normal.Add(double.Parse(stlLine.Substring(11, 14))); // x2
                            normal.Add(double.Parse(stlLine.Substring(25, 14))); // y2
                            normal.Add(double.Parse(stlLine.Substring(39, 14))); // z2
                            normal.Add(double.Parse(stlLine.Substring(11, 14))); // x3
                            normal.Add(double.Parse(stlLine.Substring(25, 14))); // y3
                            normal.Add(double.Parse(stlLine.Substring(39, 14))); // z3
                            //----------------------------------------------------------------------
                            stlLine = stlReader.ReadLine().Trim().Replace(" ", ""); // OuterLoop
                            //----------------------------------------------------------------------
                            // Vertex1
                            stlLine = stlReader.ReadLine().Trim().Replace(" ", "");
                            triangle.Add(double.Parse(stlLine.Substring(6, 14))); // x1
                            triangle.Add(double.Parse(stlLine.Substring(20, 14))); // y1
                            triangle.Add(double.Parse(stlLine.Substring(34, 14))); // z1
                            // Vertex2
                            stlLine = stlReader.ReadLine().Trim().Replace(" ", "");
                            triangle.Add(double.Parse(stlLine.Substring(6, 14))); // x2
                            triangle.Add(double.Parse(stlLine.Substring(20, 14))); // y2
                            triangle.Add(double.Parse(stlLine.Substring(34, 14))); // z2
                            // Vertex3
                            stlLine = stlReader.ReadLine().Trim().Replace(" ", "");
                            triangle.Add(double.Parse(stlLine.Substring(6, 14))); // x3
                            triangle.Add(double.Parse(stlLine.Substring(20, 14))); // y3
                            triangle.Add(double.Parse(stlLine.Substring(34, 14))); // z3
                            //----------------------------------------------------------------------
                            stlLine = stlReader.ReadLine().Trim().Replace(" ", ""); // EndLoop
                            //----------------------------------------------------------------------
                            stlLine = stlReader.ReadLine().Trim().Replace(" ", ""); // endFacet

                            i += 9;
                        }
                        break;

                        default:  // if user selects wrong file, application restarts
                            MessageBox.Show("Wrong file selected!\nProgram will restart!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Application.Restart();
                            Environment.Exit(0);             
                        break;
                }
            }

            i -= 9; // in the last cycle of while loop 'i' value increased unnecessarily, thats why we need to substract 9 to 'i' value 
            numberOfCoords = i;
            stlReader.Close(); // close the *.txt file
        }
      
        public void drawGl()
        {
            keys();
            glDynamic(glMonitor);
            //----------------------------------------------------------------------          
            Gl.glPushMatrix();
            Gl.glTranslated(tX, tY, 0);
            Gl.glScaled(scale, scale, scale);
            Gl.glRotated(rotX, 1, 0, 0);  
            Gl.glRotated(rotY, 0, 1, 0);  
            Gl.glRotated(rotZ, 0, 0, 1);          
            Gl.glColor3d(0.118, 0.565, 1.000); // set model color 
            Gl.glEnableClientState(Gl.GL_VERTEX_ARRAY);
            Gl.glEnableClientState(Gl.GL_NORMAL_ARRAY);
            Gl.glVertexPointer(3, Gl.GL_DOUBLE, 0, vaoTriangle);                                    
            Gl.glNormalPointer(Gl.GL_DOUBLE, 0, vaoNormal);          
            Gl.glDrawArrays(Gl.GL_TRIANGLES, 0, numberOfCoords / 3); // Every 3 coordinates creates a 3d vertex        
            Gl.glDisableClientState(Gl.GL_NORMAL_ARRAY);
            Gl.glDisableClientState(Gl.GL_VERTEX_ARRAY);
            Gl.glPopMatrix();
            //----------------------------------------------------------------------
            glMonitor.Invalidate();
        }

        private void glMonitor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W)
            {
                keyW = true;
            }
            if (e.KeyCode == Keys.S)
            {
                keyS = true;
            }
            if (e.KeyCode == Keys.D)
            {
                keyD = true;
            }
            if (e.KeyCode == Keys.A)
            {
                keyA = true;
            }
            if (e.KeyCode == Keys.Q)
            {
                keyQ = true;
            }
            if (e.KeyCode == Keys.E)
            {
                keyE = true;
            }

            if (e.KeyCode == Keys.Add)
            {
                keyAdd = true;
            }

            if (e.KeyCode == Keys.Subtract)
            {
                keySub = true;
            }
        }

        private void glMonitor_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W)
            {
                keyW = false;
            }
            if (e.KeyCode == Keys.S)
            {
                keyS = false;
            }
            if (e.KeyCode == Keys.D)
            {
                keyD = false;
            }
            if (e.KeyCode == Keys.A)
            {
                keyA = false;
            }
            if (e.KeyCode == Keys.Q)
            {
                keyQ = false;
            }
            if (e.KeyCode == Keys.E)
            {
                keyE = false;
            }

            if (e.KeyCode == Keys.Add)
            {
                keyAdd = false;
            }

            if (e.KeyCode == Keys.Subtract)
            {
                keySub = false;
            }
        }

        private void rotXBt_Click(object sender, EventArgs e)
        {
            rotX += 3;
        }

        private void rotXeksiBt_Click(object sender, EventArgs e)
        {
            rotX -= 3;
        }

        private void rotYBt_Click(object sender, EventArgs e)
        {
            rotY += 3;
        }

        private void rotYeksiBt_Click(object sender, EventArgs e)
        {
            rotY -= 3;
        }


        private void rotZBt_Click(object sender, EventArgs e)
        {
            rotZ += 3;
        }

        private void rotZeksiBt_Click(object sender, EventArgs e)
        {
            rotZ -= 3;           
        }

        private void zoominBt_Click(object sender, EventArgs e)
        {
      
            scale += 0.1;
        }

        private void zoomoutBt_Click(object sender, EventArgs e)
        {
            if (scale > 0.1)
            {
                scale -= 0.1;
            }
        }

        private void PanUpBt_Click(object sender, EventArgs e)
        {
            tY -= 3;
        }

        private void PanDownBt_Click(object sender, EventArgs e)
        {
            tY += 3;
        }

        private void panLeftBt_Click(object sender, EventArgs e)
        {
            tX += 3;
        }

        private void PanRightBt_Click(object sender, EventArgs e)
        {
            tX -= 3;
        }

        void keys()
        {
            if (keyW)
            { rotXBt.PerformClick(); }
            if (keyA)
            { rotZBt.PerformClick(); }
            if (keyS)
            { rotXeksiBt.PerformClick(); }
            if (keyD)
            { rotZeksiBt.PerformClick(); }
            if (keyQ)
            { rotYBt.PerformClick(); }
            if (keyE)
            { rotYeksiBt.PerformClick(); }
            if (keyAdd)
            { zoominBt.PerformClick(); }
            if (keySub)
            { zoomoutBt.PerformClick(); }
        }

        private void selectFileBt_Click(object sender, EventArgs e)
        {
            OpenFileDialog selectStlFile = new OpenFileDialog();
            selectStlFile.Filter = "TXT|*.txt";
         
            if(selectStlFile.ShowDialog() == DialogResult.OK)
            {
                selectFileTxb.Text = selectStlFile.SafeFileName;               
                try
                {
                    readStl(selectStlFile.FileName);
                    vaoNormal = new double[normal.Count];
                    vaoNormal = normal.ToArray();
                    vaoTriangle = new double[triangle.Count];
                    vaoTriangle = triangle.ToArray();
                    normal.Clear(); // after defining the proper lenght of arrays, lists will clear
                    triangle.Clear();
                    GC.Collect();
                    monitorRefreshTimer.Enabled = true; // start drawing 
                }
                catch
                {
                    MessageBox.Show("Wrong file selected!\nProgram will restart!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Restart();
                    Environment.Exit(0);                   
                }
            }

        }

        private void selectFileTxb_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true; // disable textbox user input
        }

        private void monitorRefreshTimer_Tick(object sender, EventArgs e)
        {
            drawGl();
        }

        private void exitBt_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

    }
}