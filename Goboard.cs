/**
 *  Go Applet
 *  1996.11		xinz	written in Java
 *  2001.3		xinz	port to C#
 *  2001.5.10	xinz	file parsing, back/forward
 */

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace Go_WinApp
{
    /// <summary>
    /// The Black color and the white color
    /// </summary>
	public enum StoneColor : byte
	{
		black = 0, white = 1
	}


	/// <summary>
	/// The Controller and Viewer
	/// </summary>
	public class GoBoard : System.Windows.Forms.Form
	{
		string [] strLabels; 

		int nSize;		                // There are 19 lines on the chess board
		const int nBoardMargin = 10;	
		int nCoodStart = 7;
		const int	nBoardOffset = 20;
		int nEdgeLen = nBoardOffset + nBoardMargin;
		int nTotalGridWidth = 360 + 36;	// The size of the chess board
		int nUnitGridWidth = 22;		// The size of the stone
		int nSeq = -1;
		Rectangle rGrid;		    // The chess board including the whole orange areas
		StoneColor m_colorToPlay;   // The color to set up in the move 
		GoMove m_gmLastMove;	    // The last move 
		Boolean bDrawMark;	        // When it's true, draw the mark 
		Boolean m_fAnyKill;	        // Put a stone and then we will see if there are any kills
		Spot [,] Grid;		        // Every spot is a position to put the stone
		Pen penGrid, penStoneW, penStoneB,penMarkW, penMarkB;
		Brush brStar, brBoard, brBlack, brWhite, m_brMark;
	
       
        int nFFMove = 10;   // Go 10 moves one time
        int nRewindMove = 10;  // Back 10 moves one time 

		GoTree	gameTree;

		/// <value>
		/// The components of the UI
        /// Like the button and textbox
		/// </value>
		private System.ComponentModel.Container components;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.Button Rewind;
		private System.Windows.Forms.Button FForward;
		private System.Windows.Forms.Button Save;
		private System.Windows.Forms.Button Open;
		private System.Windows.Forms.Button Back;
		private System.Windows.Forms.Button Forward;

        /// <summary>
        /// Create a Board with the provided size
        /// </summary>
        /// <param name="nSize">19 lines is a normal one</param>
		public GoBoard(int nSize)
		{
			// Do some initial work with the components
			InitializeComponent();

            // Set the number of lines with the provided number
			this.nSize = nSize;  

			m_colorToPlay = StoneColor.black;

			Grid = new Spot[nSize,nSize];
			for (int i=0; i<nSize; i++)
				for (int j=0; j<nSize; j++)
					Grid[i,j] = new Spot();
			penGrid = new Pen(Color.Brown, (float)0.5);
			penStoneW = new Pen(Color.WhiteSmoke, (float)1);
			penStoneB = new Pen(Color.Black,(float)1);
			penMarkW = new Pen(Color.Blue, (float) 1);
			penMarkB = new Pen(Color.Beige, (float) 1);

			brStar = new SolidBrush(Color.Black);
			brBoard = new SolidBrush(Color.Orange);
			brBlack = new SolidBrush(Color.Black);
			brWhite = new SolidBrush(Color.White);
			m_brMark = new SolidBrush(Color.Red);

			rGrid = new Rectangle(nEdgeLen, nEdgeLen,nTotalGridWidth, nTotalGridWidth);
			strLabels = new string [] {"a","b","c","d","e","f","g","h","i","j","k","l","m","n","o","p","q","r","s","t"};
			gameTree = new GoTree();
		}

		// Do the UI work.
		private void InitializeComponent()
        {

            this.Open = new System.Windows.Forms.Button();
            this.Save = new System.Windows.Forms.Button();
            this.Rewind = new System.Windows.Forms.Button();
            this.Forward = new System.Windows.Forms.Button();
            this.Back = new System.Windows.Forms.Button();
            this.FForward = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            
            // Open
            // Set the properties for the Open button.
            this.Open.Location = new System.Drawing.Point(534, 95);
            this.Open.Name = "Open";
            this.Open.Size = new System.Drawing.Size(67, 25);
            this.Open.TabIndex = 2;
            this.Open.Text = "open";
            this.Open.Click += new System.EventHandler(this.Open_Click);
             
            // Save
            // Set the properties for the Save button.
            this.Save.Location = new System.Drawing.Point(611, 95);
            this.Save.Name = "Save";
            this.Save.Size = new System.Drawing.Size(67, 25);
            this.Save.TabIndex = 3;
            this.Save.Text = "save";
            this.Save.Click += new System.EventHandler(this.Save_Click);
            
            
            
            // Forward
            // Set the properties for the Forward button.
            this.Forward.Location = new System.Drawing.Point(534, 26);
            this.Forward.Name = "Forward";
            this.Forward.Size = new System.Drawing.Size(67, 25);
            this.Forward.TabIndex = 0;
            this.Forward.Text = ">";
            this.Forward.Click += new System.EventHandler(this.Forward_Click);
            
            // Back
            // Set the properties for the Back button.
            this.Back.Location = new System.Drawing.Point(611, 26);
            this.Back.Name = "Back";
            this.Back.Size = new System.Drawing.Size(67, 25);
            this.Back.TabIndex = 1;
            this.Back.Text = "<";
            this.Back.Click += new System.EventHandler(this.Back_Click);
            
            // FForward
            // Set the properties for the FForward button.
            this.FForward.Location = new System.Drawing.Point(534, 60);
            this.FForward.Name = "FForward";
            this.FForward.Size = new System.Drawing.Size(67, 25);
            this.FForward.TabIndex = 4;
            this.FForward.Text = ">>";
            this.FForward.Click += new System.EventHandler(this.FForward_Click);

            // Rewind
            // Set the properties for the Rewind button.
            this.Rewind.Location = new System.Drawing.Point(611, 60);
            this.Rewind.Name = "Rewind";
            this.Rewind.Size = new System.Drawing.Size(67, 25);
            this.Rewind.TabIndex = 5;
            this.Rewind.Text = "<<";
            this.Rewind.Click += new System.EventHandler(this.Rewind_Click);
             
            // textBox1
            // Set the properties for the textBox1.
            this.textBox1.Location = new System.Drawing.Point(536, 138);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(144, 335);
            this.textBox1.TabIndex = 6;
            this.textBox1.Text = "please oepn a .sgf file to view, or just play on the board";
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);

            // GoBoard
            // Set the properties for the Game showing.
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(700, 495);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.Rewind);
            this.Controls.Add(this.FForward);
            this.Controls.Add(this.Save);
            this.Controls.Add(this.Open);
            this.Controls.Add(this.Back);
            this.Controls.Add(this.Forward);
            this.Name = "GoBoard";
            this.Text = "Go_WinForm";
            this.Load += new System.EventHandler(this.GoBoard_Load);
            this.Click += new System.EventHandler(this.GoBoard_Click);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.PaintHandler);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.MouseUpHandler);
            this.ResumeLayout(false);
            this.PerformLayout();

		} 

        // Put the stone 
		private void MouseUpHandler(Object sender,MouseEventArgs e)
		{
			Point p;
			GoMove	gmThisMove;

			p = PointToGrid(e.X,e.Y);
			if (!onBoard(p.X, p.Y) || !closeEnough(p,e.X, e.Y)|| Grid[p.X,p.Y].hasStone())
				return; // It can't be put when it is Not onboard or Not closeEnough or there has been a stone

			// Create a new move and then play it
			gmThisMove = new GoMove(p.X, p.Y, m_colorToPlay, 0);
			playNext(ref gmThisMove);
			gameTree.addMove(gmThisMove);
		}

        #region MouseUpHandler
        // Judge if the (x, y) is inside the board
        private Boolean onBoard(int x, int y)
        {
            return (x >= 0 && x < nSize && y >= 0 && y < nSize);
        }


        // This makes the (x,y) to the point in grid
        private Point PointToGrid(int x, int y)
        {
            Point p = new Point(0, 0);
            p.X = (x - rGrid.X + nUnitGridWidth / 2) / nUnitGridWidth;
            p.Y = (y - rGrid.Y + nUnitGridWidth / 2) / nUnitGridWidth;
            return p;
        }

        // Judge if the point is close enough with the (x, y)
        // If not then nothing is gonna happen
        private Boolean closeEnough(Point p, int x, int y)
        {
            if (x < rGrid.X + nUnitGridWidth * p.X - nUnitGridWidth / 3 ||
                x > rGrid.X + nUnitGridWidth * p.X + nUnitGridWidth / 3 ||
                y < rGrid.Y + nUnitGridWidth * p.Y - nUnitGridWidth / 3 ||
                y > rGrid.Y + nUnitGridWidth * p.Y + nUnitGridWidth / 3)
            {
                return false;
            }
            else
                return true;
        }
        #endregion

        /// <summary>
        /// Play the next move
        /// Show on the screen
        /// </summary>
        /// <param name="gm"></param>
		public void playNext(ref GoMove gm) 
		{
			Point p = gm.Point;
			m_colorToPlay = gm.Color;	// The stone is going to be put with the color

			// New marks and labels coming So clear the old one
			clearLabelsAndMarksOnBoard(); 
			
			if (m_gmLastMove != null)
				repaintOneSpotNow(m_gmLastMove.Point);

			bDrawMark = true;
			Grid[p.X,p.Y].setStone(gm.Color);
			m_gmLastMove = new GoMove(p.X, p.Y, gm.Color, ++nSeq);
			// Set the new labels and marks
			setLabelsOnBoard(gm);
			setMarksOnBoard(gm);
			
            // What will happen if we put a new stone?
			doDeadGroup(nextTurn(m_colorToPlay));
			
			if (m_fAnyKill)
				appendDeadGroup(ref gm, nextTurn(m_colorToPlay));
		/*	else 
			{
				doDeadGroup(m_colorToPlay);
				if (m_fAnyKill)
					appendDeadGroup(ref gm, m_colorToPlay);
			}
		*/
            m_fAnyKill = false;
			
			optRepaint();

            // Clear the textbox and show some new comments
            textBox1.Clear();
            textBox1.AppendText(gm.Comment);
            
            // And now it's a new turn
			m_colorToPlay = nextTurn(m_colorToPlay);
			
			
		}

      
        #region PlayNext
        // Clear all the labels and the marks on the borad 
        private void clearLabelsAndMarksOnBoard()
        {
            for (int i = 0; i < nSize; i++)
                for (int j = 0; j < nSize; j++)
                {
                    if (Grid[i, j].hasLabel())
                        Grid[i, j].resetLabel();
                    if (Grid[i, j].hasMark())
                        Grid[i, j].resetMark();
                }
        }

        // Repaint the area where the Point p locates.
        private void repaintOneSpotNow(Point p)
        {
            Grid[p.X, p.Y].setUpdated();
     //       bDrawMark = false;
            Rectangle r = getUpdatedArea(p.X, p.Y);
            Invalidate(new Region(r));
            Grid[p.X, p.Y].resetUpdated();
            bDrawMark = true;
        }

        // The next color to put in the next turn
        private StoneColor nextTurn(StoneColor c)
        {
            if (c == StoneColor.black)
                return StoneColor.white;
            else
                return StoneColor.black;
        }

        private void setLabelsOnBoard(GoMove gm)
        {
            short nLabel = 0;
            Point p;
            if (null != gm.Labels)
            {
                int i = gm.Labels.Count;
                i = gm.Labels.Capacity;

                System.Collections.IEnumerator myEnumerator = gm.Labels.GetEnumerator();
                while (myEnumerator.MoveNext())
                {
                    p = (Point)myEnumerator.Current;
                    Grid[p.X, p.Y].setLabel(++nLabel);
                }
            }
        }

        // Kill all the certain color stones whose liberty is 0
        void doDeadGroup(StoneColor c)
        {
            int i, j;
            for (i = 0; i < nSize; i++)
                for (j = 0; j < nSize; j++)
                    if (Grid[i, j].hasStone() &&
                        Grid[i, j].color() == c)
                    {
                        if (calcLiberty(i, j, c) == 0)
                        {
                            buryTheDead(i, j, c);
                            m_fAnyKill = true;
                        }
                        cleanScanStatus();
                    }
        }

        // The tool to clear all the scan status
        void cleanScanStatus()
        {
            int i, j;
            for (i = 0; i < nSize; i++)
                for (j = 0; j < nSize; j++)
                    Grid[i, j].clearScanned();
        }

        // Every move has a dead group
		private void appendDeadGroup(ref GoMove gm, StoneColor c)
		{
			for (int i=0; i<nSize; i++)
				for (int j=0; j<nSize; j++)
					if (Grid[i,j].isKilled())
					{
						Point pt = new Point(i,j);
						gm.DeadGroup.Add(pt);
						Grid[i,j].setNoKilled();
					}
			gm.DeadGroupColor = c;
		}

        // Calculate how many Liberties the Point(x,y) has. 
        private int calcLiberty(int x, int y, StoneColor c)
        {
            int lib = 0; // The liberty it has

            if (!onBoard(x, y))
                return 0;
            if (Grid[x, y].isScanned())
                return 0;

            if (Grid[x, y].hasStone())
            {
                if (Grid[x, y].color() == c)
                {
                    // Make sure not to add again
                    Grid[x, y].setScanned();
                    lib += calcLiberty(x - 1, y, c);
                    lib += calcLiberty(x + 1, y, c);
                    lib += calcLiberty(x, y - 1, c);
                    lib += calcLiberty(x, y + 1, c);
                }
                else
                    return 0;
            }
            else
            {// plus the liberty when it ts empty
                lib++;
                Grid[x, y].setScanned();
            }

            return lib;
        }

        // Set the killed group for the color c
        // Well Obviously,in a group when one is dead then others are dead too
        private void buryTheDead(int i, int j, StoneColor c)
        {
            if (onBoard(i, j) && Grid[i, j].hasStone() &&
                Grid[i, j].color() == c)
            {
                Grid[i, j].die();
                buryTheDead(i - 1, j, c);
                buryTheDead(i + 1, j, c);
                buryTheDead(i, j - 1, c);
                buryTheDead(i, j + 1, c);
            }
        }

        

        // Get the rectangle to update with the row and col
        private Rectangle getUpdatedArea(int i, int j)
        {
            int x = rGrid.X + i * nUnitGridWidth - nUnitGridWidth / 2;
            int y = rGrid.Y + j * nUnitGridWidth - nUnitGridWidth / 2;
            return new Rectangle(x, y, nUnitGridWidth, nUnitGridWidth);
        }

        // Repaint the area that needs to update.
        private void optRepaint()
        {
            Rectangle r = new Rectangle(0, 0, 0, 0);
            Region re;

            for (int i = 0; i < nSize; i++)
                for (int j = 0; j < nSize; j++)
                    if (Grid[i, j].isUpdated())
                    {
                        r = getUpdatedArea(i, j);
                        re = new Region(r);
                        Invalidate(re);
                    }
           
        }


        #endregion


        
		/// <summary>
		/// Play the previous move
		/// </summary>
		/// <param name="gm"></param>
		public void playPrev(GoMove gm)
		{
           
            Point p = m_gmLastMove.Point;

            // New marks and labels coming So clear the old one
            clearLabelsAndMarksOnBoard();
            Grid[p.X, p.Y].removeStone();
            if (m_gmLastMove != null)
                repaintOneSpotNow(m_gmLastMove.Point);

            
            m_gmLastMove = gm;
            // Set the new labels and marks
            setLabelsOnBoard(gm);
            setMarksOnBoard(gm);

            Debug.Assert(gameTree.peekNext() != null);
            Debug.Assert(gameTree.peekNext().DeadGroup != null);
            if (gameTree.peekNext().DeadGroup.Count > 0)
                reliveTheKilled(gameTree.peekNext());
            
      //      optRepaint();

            // Clear the textbox and show some new comments
            textBox1.Clear();
            textBox1.AppendText(gm.Comment);

        }

        // Relive the killed stones
        private void reliveTheKilled(GoMove gm)
        {
            ArrayList toRelive = gm.DeadGroup;
            foreach (Point p in toRelive)
            {
                Grid[p.X, p.Y].setStone(nextTurn(gm.Color));
            }

        }

        /*
		//ZZZZ ZZ ZZZZZZZZ ZZ ZZZZZZZ ZZZ ZZZ ZZZZ ZZZZ ZZZZ.  
		private void recordMove(Point p, StoneColor colorToPlay) 
		{
			Grid[p.X,p.Y].setStone(colorToPlay);
			// ZZZZZZ ZZZZ ZZZZ.
			m_gmLastMove = new GoMove(p.X, p.Y, colorToPlay, nSeq++);
		}
        */		

        // Update the condition of the chess board.
		private void UpdateGoBoard(PaintEventArgs e)
		{
			DrawBoard(e.Graphics);
			
			// Draw the stars on the board.
            // That's the rules of the chess.
			drawStars(e.Graphics);

			// Draw the stones and marks and labels
			drawEverySpot(e.Graphics);
		}

        
        #region UpdateGoBoard
        
        // Draw the chess board.
        private void DrawBoard(Graphics g)
        {
            // The outside labels of the chess board
            string[] strV = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19" };
            string[] strH = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T" };

            Point p1 = new Point(nEdgeLen, nEdgeLen);
            Point p2 = new Point(nTotalGridWidth + nEdgeLen, nEdgeLen);
            g.FillRectangle(brBoard, nBoardOffset, nBoardOffset, nTotalGridWidth + nBoardOffset, nTotalGridWidth + nBoardOffset);
            for (int i = 0; i < nSize; i++)
            {
                g.DrawString(strV[i], this.Font, brBlack, 0, nCoodStart + nBoardOffset + nUnitGridWidth * i);
                g.DrawString(strH[i], this.Font, brBlack, nBoardOffset + nCoodStart + nUnitGridWidth * i, 0);
                g.DrawLine(penGrid, p1, p2);
                g.DrawLine(penGrid, SwapXY(p1), SwapXY(p2));

                p1.Y += nUnitGridWidth;
                p2.Y += nUnitGridWidth;
            }
            // The edge of the chess board
            Pen penHi = new Pen(Color.WhiteSmoke, (float)0.5);
            Pen penLow = new Pen(Color.Gray, (float)0.5);

            g.DrawLine(penHi, nBoardOffset, nBoardOffset, nTotalGridWidth + 2 * nBoardOffset, nBoardOffset);
            g.DrawLine(penHi, nBoardOffset, nBoardOffset, nBoardOffset, nTotalGridWidth + 2 * nBoardOffset);
            g.DrawLine(penLow, nTotalGridWidth + 2 * nBoardOffset, nTotalGridWidth + 2 * nBoardOffset, nBoardOffset + 1, nTotalGridWidth + 2 * nBoardOffset);
            g.DrawLine(penLow, nTotalGridWidth + 2 * nBoardOffset, nTotalGridWidth + 2 * nBoardOffset, nTotalGridWidth + 2 * nBoardOffset, nBoardOffset + 1);
        }		

		/// Draw the stone in a position with the certain color.
		private void drawStone(Graphics g, int row, int col, StoneColor c) 
		{
			Brush br;
			if (c == StoneColor.white)
				br = brWhite;
			else 
				br = brBlack;
			
			Rectangle r = new Rectangle(rGrid.X+ (row) * nUnitGridWidth - (nUnitGridWidth-1)/2, 
				rGrid.Y + (col) * nUnitGridWidth - (nUnitGridWidth-1)/2,
				nUnitGridWidth-1,
				nUnitGridWidth-1);

			g.FillEllipse(br, r);
		}

        // Draw 9 stars on the board. 
        private void drawStars(Graphics g)
        {
            drawStar(g, 4, 4);
            drawStar(g, 4, 10);
            drawStar(g, 4, 16);
            drawStar(g, 10, 4);
            drawStar(g, 10, 10);
            drawStar(g, 10, 16);
            drawStar(g, 16, 4);
            drawStar(g, 16, 10);
            drawStar(g, 16, 16);
        }
        int count = 0;
        private void drawEverySpot(Graphics g)
        {
            // Mark the stone that makes it eye-watching.
            if (bDrawMark && m_gmLastMove != null)
            {
                markLastMove(g);
                Debug.Assert(Grid[m_gmLastMove.Point.X - 3, m_gmLastMove.Point.Y].hasStone() == true);
        //        Thread.Sleep(2000);

        //        Grid[0, 0].setStone(StoneColor.black);
            }
            
            for (int i = 0; i < nSize; i++)
                for (int j = 0; j < nSize; j++)
                {
                    if (Grid[i, j].hasStone())
                    {
                        count++;
                        drawStone(g, i, j, Grid[i, j].color());

                    } if (Grid[i, j].hasLabel())
                        drawLabel(g, i, j, Grid[i, j].getLabel());
                    if (Grid[i, j].hasMark())
                        drawMark(g, i, j);
                }
            g.DrawString(count.ToString(), this.Font, brBlack,new PointF(600, 120));
            g.FillRectangle(count%4==0?brBlack:brWhite, new Rectangle(550, 120, 5, 5));
        }
		private void drawLabel(Graphics g, int x, int y, short nLabel) 
		{
			if (nLabel ==0)
				return;
			nLabel --;
			nLabel %= 18;			

			Rectangle r = new Rectangle(rGrid.X+ x * nUnitGridWidth - (nUnitGridWidth-1)/2, 
				rGrid.Y + y * nUnitGridWidth - (nUnitGridWidth-1)/2,
				nUnitGridWidth-1,
				nUnitGridWidth-1);

			g.FillEllipse(brBlack, r);

			g.DrawString(strLabels[nLabel],	
				this.Font, 
				brBlack, 
				rGrid.X+ (x) * nUnitGridWidth - (nUnitGridWidth-1)/4, 
				rGrid.Y + (y) * nUnitGridWidth - (nUnitGridWidth-1)/2);
		}

		private void drawMark(Graphics g, int x, int y)
		{
			g.FillRectangle( m_brMark,
				rGrid.X + x* nUnitGridWidth - (nUnitGridWidth-1)/8, 
				rGrid.Y + y * nUnitGridWidth - (nUnitGridWidth-1)/8,
				5, 5);
		}

		

        // Mark the stone that makes it eye-watching.
        private void markLastMove(Graphics g)
        {
            Point p = m_gmLastMove.Point;
            Grid[p.X-3, p.Y].setStone(StoneColor.white);
            Brush brMark;
            if (m_gmLastMove.Color == StoneColor.white)
                brMark = brBlack;
            else
                brMark = brWhite;
            g.FillRectangle(brMark,
                rGrid.X + (p.X) * nUnitGridWidth - (nUnitGridWidth - 1) / 8,
                rGrid.Y + (p.Y) * nUnitGridWidth - (nUnitGridWidth - 1) / 8,
                3, 3);
        }

        // Draw the star in a certain row and col.
        private void drawStar(Graphics g, int row, int col)
        {
            g.FillRectangle(brStar,
                rGrid.X + (row - 1) * nUnitGridWidth - 1,
                rGrid.Y + (col - 1) * nUnitGridWidth - 1,
                3,
                3);
        }

        private void setMarksOnBoard(GoMove gm)
        {
            Point p;
            if (null != gm.Labels)
            {
                System.Collections.IEnumerator myEnumerator = gm.Marks.GetEnumerator();
                while (myEnumerator.MoveNext())
                {
                    p = (Point)myEnumerator.Current;
                    Grid[p.X, p.Y].setMark();
                }
            }
        }
        #endregion
        

		/// Load the sgf file to show the chess manual.
		private void OpenFile()
		{
			OpenFileDialog openDlg = new OpenFileDialog();
			openDlg.Filter  = "sgf files (*.sgf)|*.sgf|All Files (*.*)|*.*";
			openDlg.FileName = "" ;
			openDlg.DefaultExt = ".sgf";
			openDlg.CheckFileExists = true;
			openDlg.CheckPathExists = true;
			
			DialogResult res = openDlg.ShowDialog ();
			
			if(res == DialogResult.OK)
			{
				if( !(openDlg.FileName).EndsWith(".sgf") && !(openDlg.FileName).EndsWith(".SGF")) 
					MessageBox.Show("Unexpected file format","Super Go Format",MessageBoxButtons.OK);
				else
				{
					FileStream f = new FileStream(openDlg.FileName, FileMode.Open); 
					StreamReader r = new StreamReader(f);
					string s = r.ReadToEnd();
					gameTree = new GoTree(s);
					gameTree.reset();
                    resetBoard();
					r.Close(); 
					f.Close();
				}
			}		
		}

        #region tools
        #region handlers

        private void GoBoard_Load(object sender, EventArgs e)
        {

        }

        protected void textBox1_TextChanged(object sender, System.EventArgs e)
        {
            return;
        }

        private void PaintHandler(Object sender, PaintEventArgs e)
        {
            UpdateGoBoard(e);
        }

        protected void Save_Click(object sender, System.EventArgs e)
        {
            return;
        }

        protected void Open_Click(object sender, System.EventArgs e)
        {
            OpenFile();
            showGameInfo();
        }

        protected void Rewind_Click(object sender, System.EventArgs e)
        {
            gameTree.reset();
            resetBoard();
            showGameInfo();
        }

        protected void FForward_Click(object sender, System.EventArgs e)
        {
            if (gameTree != null)
            {
                int i = 0;
                GoMove gm = null;
                for (gm = gameTree.doNext(); gm != null; gm = gameTree.doNext())
                {
                    playNext(ref gm);
                    if (i++ > nFFMove)
                        break;
                }
            }
        }

        protected void Forward_Click(object sender, System.EventArgs e)
        {
            GoMove gm = gameTree.doNext();
            if (null != gm)
            {
                playNext(ref gm);
            }
        }
        protected void Back_Click(object sender, System.EventArgs e)
        {
            GoMove gm = gameTree.doPrev();	
            if (null != gm)
            {
                playPrev(gm);
            }
            else
            {
                resetBoard();
                gameTree.reset();
                showGameInfo();
            }
        }
        protected void GoBoard_Click(object sender, System.EventArgs e)
        {
            return;
        }

        
        #endregion

        // Show the game information
        private void showGameInfo()
        {
            // Clear the textbox and show the new game info
            textBox1.Clear();
            textBox1.AppendText(gameTree.Info);
        }

        // This is used to reset the game
        private void resetBoard()
        {
            int i, j;
            for (i = 0; i < nSize; i++)
                for (j = 0; j < nSize; j++)
                    Grid[i, j].removeStone();
            m_gmLastMove = null;
            Invalidate(null);
        }

        private Point SwapXY(Point p)
        {
            Point pNew = new Point(p.Y, p.X);
            return pNew;
        }
        #endregion
    }

	

	
	/// <summary>
	/// The every spot in chess board
    /// It has many properties like emptity/deadlive/scan/update
	/// </summary>
	public class Spot 
	{
		private Boolean bEmpty;
		private Boolean bKilled;
		private Stone s;
		private short	m_nLabel;
		private Boolean m_bMark;
		private Boolean bScanned;
		private Boolean bUpdated; // If yes then then next draw will update the spot
		
		public Spot() 
		{
			bEmpty = true;
			bScanned = false;
			bUpdated = false;
			bKilled = false;
		}
		
		public Boolean hasStone() { return !bEmpty;	}
		public Boolean isEmpty() {	return bEmpty;	}
		public Stone thisStone() {	return s;}
		public StoneColor color() {	return s.color;}

		public Boolean hasLabel() {return m_nLabel>0;}
		public Boolean hasMark() {return m_bMark;}
		public void setLabel(short l) {m_nLabel = l; bUpdated = true; }
		public void setMark() {m_bMark = true; bUpdated = true;}
		public void resetLabel() {m_nLabel = 0; bUpdated = true;}
		public void resetMark() {m_bMark = false; bUpdated = true;}
		public short	getLabel() {return m_nLabel;}

		public Boolean isScanned() { return bScanned;}
		public void setScanned() {	bScanned = true;}
		public void clearScanned() { bScanned = false; }

        /// <summary>
        /// Put the stone with the color c on the board
        /// </summary>
        /// <param name="c"></param>
		public void setStone(StoneColor c) 
		{
			if (bEmpty) 
			{
				bEmpty = false;
				s.color = c;
				bUpdated = true;
			} // if yes then put the stone on the board with the color c and update in the next draw 
		}

		/// <summary>
		/// Remove the stone on the board
		/// </summary>
		public void removeStone()
		{	// remove stone means set the properties again
			bEmpty = true;
			bUpdated = true;
		}
				
		/// <summary>
        /// Kill some stones
		/// </summary>
		public void die() 
		{
			bKilled = true;
			bEmpty = true;
			bUpdated = true;
		}

		public Boolean isKilled() { return bKilled;}
		public void setNoKilled() { bKilled = false;}

		public void resetUpdated() { bUpdated = false; bKilled = false;}

		/// <summary>
		/// Judge the spot if it's going to update
		/// </summary>
		/// <returns></returns>
		public Boolean isUpdated() 
		{ 
			if (bUpdated)
			{	
				bUpdated = false;
				return true;
			} 
			else 
				return false;
		}

		/// <summary>
		/// This spot is going to update
		/// </summary>
		public void setUpdated() { bUpdated = true; }
	}

	/// <summary>
	/// The every move means every turn in the real game
	/// </summary>
	public class GoMove 
	{
		StoneColor m_c;	// The color to play in this move
        Point m_pos;		// The point to put in this move
		int m_n;			// The turn of the move
		String m_comment;	// Every move has a comment
		MoveResult m_mr;	// Nothing happen 

		ArrayList		m_alLabel; // Every move has labels 
		ArrayList		m_alMark; // Every move has marks

		// The group of the killed stones in this move
		// The color of the stone that's killed 
		ArrayList		m_alDead;
		StoneColor	m_cDead;
		
        /// <summary>
        /// Create a new move with the (x,y) and color 
        /// </summary>
        /// <param name="x">The row to put</param>
        /// <param name="y">The col to put</param>
        /// <param name="sc">The color to put</param>
        /// <param name="seq"></param>
		public GoMove(int x, int y, StoneColor sc, int seq) 
		{
			m_pos = new Point(x,y);
			m_c = sc;
			m_n = seq;
			m_mr = new MoveResult();
			m_alLabel = new ArrayList();
			m_alMark = new ArrayList();
            m_alDead = new ArrayList();
		}

        /// <summary>
        /// Create a new move with the string and color
        /// Like "AH" will be changed to (0, 8)
        /// </summary>
        /// <param name="str">It stands for the point</param>
        /// <param name="c">The color to put</param>
		public GoMove(String str, StoneColor c) 
		{
			char cx = str[0];
			char cy = str[1];
			m_pos = new Point(0,0);  
			m_pos.X = (int) ( (int)cx - (int)(char)'a');
			m_pos.Y = (int) ( (int)cy - (int)(char)'a');
			this.m_c = c;
			m_alLabel = new ArrayList();
			m_alMark = new ArrayList();
		}

        // Change the str to the point like "A11" to (0,11).
		private Point	StrToPoint(String str)
		{
			Point p = new Point(0,0);
			char cx = str[0];
			char cy = str[1];
			
			p.X = (int) ( (int)cx - (int)(char)'a');
			p.Y = (int) ( (int)cy - (int)(char)'a');
			return p;
		}

        /*
         * The Getters and Setters of the properties
         */ 
        public StoneColor Color
        { 
            get { return m_c; } 
        }

        public String Comment 
        {
            get
            {
                if (m_comment == null)
                    return string.Empty;
                else
                    return m_comment;
            }
            set
            {
                m_comment = value; 
            }
        }

		public int Seq
        {
            get { return m_n; }
            set {	m_n = value;}
        }

        public Point Point
        {
           get  { return m_pos; }
        }

        public MoveResult Result
        {
            get { return m_mr; }
            set { m_mr = value; }
        }
		
		public ArrayList DeadGroup
        {
            get { return m_alDead;}
            set {m_alDead = value;}
        }

        public StoneColor DeadGroupColor
        {
            get { return m_cDead; }
            set { m_cDead = value; }
        }
		
		public void addLabel(String str) {m_alLabel.Add(StrToPoint(str));}
		
		public void addMark(String str) {	m_alMark.Add(StrToPoint(str));}

        public ArrayList Labels
        {
            get { return m_alLabel; }
        }

        public ArrayList Marks
        {
            get { return m_alMark; }
        }
	}
	

	/// <summary>
	/// It seems that this class doesn't make any sense
	/// </summary>
	public class MoveResult 
	{
		public StoneColor color; 
		//
		public Boolean bUpKilled;
		public Boolean bDownKilled;
		public Boolean bLeftKilled;
		public Boolean bRightKilled;
		public Boolean bSuicide;	
		public MoveResult() 
		{
			bUpKilled = false;
			bDownKilled = false;
			bLeftKilled = false;
			bRightKilled = false;
			bSuicide = false;
		}
	}

	/// <summary>
	/// Yup! It's a unuseless one
	/// </summary>
	public struct Stone 
	{
		public StoneColor color; 
	}

	/// <summary>
	/// The class controls the moves
	/// </summary>
	public class GoVariation 
	{
		int m_id;			// This is the id of one move
		ArrayList m_moves; 
		int m_seq;			// This is the turn of the move 
		int m_total;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="id"></param>
		public GoVariation(int id)
		{
			m_id = id;
			m_moves = new ArrayList(10);
			m_seq = -1;
			m_total = -1;
		}

        /// <summary>
        /// When a move completes, we add it to store it
        /// </summary>
        /// <param name="gm"></param>
		public void addAMove(GoMove gm) 
		{
            m_total++;
			m_seq++;
			m_moves.Add(gm);
		}

        /// <summary>
        /// Return a move that will play next
        /// </summary>
        /// <returns>The move which is going to play in next</returns>
		public GoMove doNext()
		{
			if (m_seq < m_total) 
			{
				return (GoMove)m_moves[++m_seq];
			} 
			else 
				return null;
		}

        /// <summary>
        /// Return a move that will play back
        /// </summary>
        /// <returns>The move which is goint to play in back</returns>
		public GoMove doPrev()
		{
			if (m_seq > 0)
				return (GoMove)(m_moves[--m_seq]);
			else 
				return null;
		}

		/// <summary>
		/// Actually I don't know what this method is going to do
		/// </summary>
		/// <returns></returns>
		public GoMove peekNext()
		{
			if (m_seq >= 0)
				return (GoMove)(m_moves[m_seq+1]);
			else 
				return null;
		}

        /// <summary>
        /// Reset the counter
        /// </summary>
		public void reset() {m_seq = -1;}
	}


	/*
     * Ooops! Another unuseless one 
     */
	struct VarStartPoint
	{
		int m_id; 
		int m_seq;
	}

    /// <summary>
    /// Some information about the game
    /// like the name,player,date and so on.
    /// </summary>
	struct GameInfo 
	{
		public string gameName;
		public string playerBlack;
		public string playerWhite;
		public string rankBlack;
		public string rankWhite;
		public string result;
		public string date;
		public string km;
		public string size;
		public string comment;
        public string handicap;
        public string gameEvent;
        public string location;
        public string time;             
        public string unknown_ff;    
        public string unknown_gm;
        public string unknown_vw; 
	}

    /// <summary>
    /// Key means the type
    /// Value means the concrete value
    /// </summary>
	public class KeyValuePair 
	{
		public string k; 
        public ArrayList alV;


		private string	removeBackSlash(string strIn)
		{
			string strOut; 
			int		iSlash;

			strOut = string.Copy(strIn);
			if (strOut.Length < 2)
				return strOut;
			for (iSlash = strOut.Length-2; iSlash>=0; iSlash--)
			{
				if (strOut[iSlash] == '\\')		
				{
					strOut = strOut.Remove(iSlash,1);
					if (iSlash>0)
						iSlash --;	
				}
			}
			return strOut;
		}

        /// <summary>
        /// The template of "A[B]".
        /// A is key, and B is value.
        /// </summary>
        /// <param name="k">k is key</param>
        /// <param name="v">v is value</param>
		public KeyValuePair(string k, string v)
		{
			this.k = string.Copy(k);
			string strOneVal;
			int		iBegin, iEnd;
		
			alV = new ArrayList(1);

			if (k.Equals("C"))
			{
				strOneVal = removeBackSlash(string.Copy(v));
				
				alV.Add(strOneVal);
				return;
			}

			iBegin = v.IndexOf("[");
			if (iBegin == -1)
			{
				alV.Add(v);
				return; 
			}
			
			iBegin = 0;
			while (iBegin < v.Length && iBegin>=0)
			{
				iEnd = v.IndexOf("]", iBegin);
				if (iEnd > 0)
					strOneVal = v.Substring(iBegin, iEnd-iBegin);
				else 
					strOneVal = v.Substring(iBegin);	
                alV.Add(strOneVal);
				iBegin = v.IndexOf("[", iBegin+1);
				if (iBegin > 0)
					iBegin ++;	
			}
		}
	}

	/// <summary>
	/// The Tree hands many moves by controlling the GoVariation class
	/// </summary>
	public class GoTree 
	{
		GameInfo _gi;		// 
		ArrayList _vars;		// This is used to store the Variations 
		int _currVarId;		// The current Variation(id)
		GoVariation _currVar;		// The current Variation(itself)
		string	_stGameComment;

		/// <summary>
		/// Create a GoTree with string
        /// The string refers to the text of a file
		/// </summary>
		/// <param name="s"></param>
		public GoTree(string s)
		{
			_vars = new ArrayList(10);
			_currVarId = 0; 
			_currVar = new GoVariation(_currVarId);
			_vars.Add(_currVar);
			parseFile(s);
		}

		/// <summary>
		/// Create a GoTree with nothing
        /// And that means no moves are created at the same time
		/// </summary>
		public GoTree()
		{
			_vars = new ArrayList(10);
			_currVarId = 0; 
			_currVar = new GoVariation(_currVarId);
			_vars.Add(_currVar);
		}

		public	string Info
		{
            get
            {
                return _gi.comment == null? string.Empty : _gi.comment;
            }
		}

		public void addMove(GoMove gm) 
		{
			_currVar.addAMove(gm);
		}

        public GoMove doPrev()
        {
            return _currVar.doPrev();
        }

        public GoMove peekNext()
        {
            return _currVar.peekNext();
        }

        public GoMove doNext()
        {
            return _currVar.doNext();
        }

        public void reset()
        {
            _currVarId = 0;
            _currVar.reset();
        }

        public void rewindToStart()
        {

        }

        
		
		/// <summary>
		/// Create some moves from the file.
		/// </summary>
		/// <param name="goStr">the file's text</param>
        private void parseFile(String goStr) 
		{
            // Split the string to ";ABA;" with the delim ';'
            // And then pass the "ABA" to parseASection
			int iBeg, iEnd=0; 
			while (iEnd < goStr.Length) 
			{
				if (iEnd > 0)
					iBeg = iEnd;
				else 
					iBeg = goStr.IndexOf(";", iEnd);
				iEnd = goStr.IndexOf(";", iBeg+1);
				if (iEnd < 0) // if we Can't find the ';', then we will find ')' 
					iEnd = goStr.LastIndexOf(")", goStr.Length);	// Find the last ')' in string goStr
				if (iBeg >= 0 && iEnd > iBeg) 
				{
					string section = goStr.Substring(iBeg+1, iEnd-iBeg-1);
					parseASection(section);
				} 
				else 
					break;
			}
		}

       	/// <summary>
        /// It's a main function of void parseFile(String).
       	/// Create some moves from the certain parts.
       	/// </summary>
       	/// <param name="sec"></param>
       	/// <returns></returns>
		private Boolean parseASection(String sec) 
		{
			int iKey = 0;
			int iValue = 0;
			int iLastValue = 0;
			KeyValuePair kv;
			ArrayList Section = new ArrayList(10);
			
			try 
			{
				iKey = sec.IndexOf("[");
				if (iKey < 0)
				{
					return false;
				}
                sec = purgeCRLFSuffix(sec);
 
				iValue = findEndofValueStr(sec); // Find the ']' in the string
				iLastValue = sec.LastIndexOf("]");
				if (iValue <= 0 || iLastValue <= 1)
				{
					return false;
				}
				sec = sec.Substring(0,iLastValue+1);
				
                // To create keyValue pairs in a part between the two ';'
                while (iKey > 0 && iValue > iKey)
				{
					string key = sec.Substring(0,iKey);
					int iNonLetter = 0;
					while (!char.IsLetter(key[iNonLetter]) && iNonLetter < key.Length)
						iNonLetter ++;
					key = key.Substring(iNonLetter);
					
					string strValue = sec.Substring(iKey+1, iValue-iKey-1);
					// Create a keyValue pair  
					kv = new KeyValuePair(key, strValue);
					Section.Add(kv);
					
                    // Go on
                    if (iValue >= sec.Length)
						break;
					sec = sec.Substring(iValue+1);
					iKey = sec.IndexOf("[");
					if (iKey > 0)
					{
						iValue = findEndofValueStr(sec); // Find a first shown ']'  
					}
				}
			}
			catch
			{
                return false;
            }

			processASection(Section);
			return true;
		}

        #region parseASection
        // Find the last ']' of "A[]B[]"
        private int findEndofValueStr(String sec)
        {
            int i = 0;
           
            while (i >= 0)
            {
                i = sec.IndexOf(']', i + 1);
                if (i > 0 && sec[i - 1] != '\\')
                    return i;    
            }
 
            return sec.Length - 1;		// If not found, return the last value
        }
/*
        // Find the last kind of B of "A[]B[]"
        private int findEndofValueStrOld(String sec)
        {
            int i = 0;
          
            bool fOutside = false;

            for (i = 0; i < sec.Length; i++)
            {
                if (sec[i] == ']')
                {
                    if (i > 1 && sec[i - 1] != '\\')
                        fOutside = true;
                }
                else if (char.IsLetter(sec[i]) && fOutside && i > 0)
                    return i - 1;
                else if (fOutside && sec[i] == '[')
                    fOutside = false;
            }
            return sec.Length - 1;		// If not found, return the last value
        }
*/
        // Return a substring which has not white spaces
        private string purgeCRLFSuffix(string inStr)
        {
            int iLast = inStr.Length - 1;  

            if (iLast <= 0)
                return inStr;

            while ((inStr[iLast] == '\r' || inStr[iLast] == '\n' || inStr[iLast] == ' '))
            {
                iLast--;
            }
            if ((iLast + 1) != inStr.Length)
                return inStr.Substring(0, iLast + 1);  
            else
                return inStr;
        }

        // Create some moves from the keyValue pairs
        private Boolean processASection(ArrayList arrKV)
        {
            Boolean fMultipleMoves = false;   // I can't see any differences
            GoMove gm = null;

            string key, strValue;

            for (int i = 0; i < arrKV.Count; i++)
            {
                key = ((KeyValuePair)(arrKV[i])).k;
                for (int j = 0; j < ((KeyValuePair)(arrKV[i])).alV.Count; j++)
                {
                    strValue = (string)(((KeyValuePair)(arrKV[i])).alV[j]);

                    if (key.Equals("B"))   // It's black stone's move
                    {
                        Debug.Assert(gm == null);
                        gm = new GoMove(strValue, StoneColor.black);
                    }
                    else if (key.Equals("W"))  // It's white stone's move
                    {
                        Debug.Assert(gm == null);
                        gm = new GoMove(strValue, StoneColor.white);
                    }
                    else if (key.Equals("C"))  // This is a comment
                    {

                        if (gm != null)
                            gm.Comment = strValue;
                        else
                            _gi.comment += strValue;
                    }
                    else if (key.Equals("L"))  // This is a label
                    {
                        if (gm != null)
                            gm.addLabel(strValue);
                        else
                            _stGameComment += strValue;
                    }

                    else if (key.Equals("M"))  // This is a mark
                    {
                        if (gm != null)
                            gm.addMark(strValue);
                        else
                            _gi.comment += strValue;
                    }
                    else if (key.Equals("AW"))		// It's white stone's multiple moves
                    {
                        fMultipleMoves = true;
                        gm = new GoMove(strValue, StoneColor.white);
                    }
                    else if (key.Equals("AB"))		// It's black stone's multiple moves
                    {
                        fMultipleMoves = true;
                        gm = new GoMove(strValue, StoneColor.black);
                    }
                    else if (key.Equals("HA"))
                        _gi.handicap = (strValue);
                    else if (key.Equals("BR"))
                        _gi.rankBlack = (strValue);
                    else if (key.Equals("PB"))
                        _gi.playerBlack = (strValue);
                    else if (key.Equals("PW"))
                        _gi.playerWhite = (strValue);
                    else if (key.Equals("WR"))
                        _gi.rankWhite = (strValue);
                    else if (key.Equals("DT"))
                        _gi.date = (strValue);
                    else if (key.Equals("KM"))
                        _gi.km = (strValue);
                    else if (key.Equals("RE"))
                        _gi.result = (strValue);
                    else if (key.Equals("SZ"))
                        _gi.size = (strValue);
                    else if (key.Equals("EV"))
                        _gi.gameEvent = (strValue);
                    else if (key.Equals("PC"))
                        _gi.location = (strValue);
                    else if (key.Equals("TM"))
                        _gi.time = (strValue);
                    else if (key.Equals("GN"))
                        _gi.gameName = strValue;

                    else if (key.Equals("FF"))
                        _gi.unknown_ff = (strValue);
                    else if (key.Equals("GM"))
                        _gi.unknown_gm = (strValue);
                    else if (key.Equals("VW"))
                        _gi.unknown_vw = (strValue);
                    else if (key.Equals("US"))
                        _gi.unknown_vw = (strValue);

                    else if (key.Equals("BS"))
                        _gi.unknown_vw = (strValue);
                    else if (key.Equals("WS"))
                        _gi.unknown_vw = (strValue);
                    else if (key.Equals("ID"))
                        _gi.unknown_vw = (strValue);
                    else if (key.Equals("KI"))
                        _gi.unknown_vw = (strValue);
                    else if (key.Equals("SO"))
                        _gi.unknown_vw = (strValue);
                    else if (key.Equals("TR"))
                        _gi.unknown_vw = (strValue);
                    else if (key.Equals("LB"))
                        _gi.unknown_vw = (strValue);
                    else if (key.Equals("RO"))
                        _gi.unknown_vw = (strValue);


                    // Unknown format
                    else
                        System.Diagnostics.Debug.Assert(false, "unhandle key: " + key + " " + strValue);

                   
                    if (fMultipleMoves)
                    {
                        _currVar.addAMove(gm);
                    }
                }
            }

            
            if (!fMultipleMoves && gm != null)
            {
                _currVar.addAMove(gm);
            }
            return true;
        } 
        #endregion

        

		
	//	public void updateResult(GoMove gm) 
	//	{
	//		_currVar.updateResult(gm);
	//	}
		
		
	} 

    /// <summary>
    /// The class to start
    /// </summary>
    public class GoTest
    {
        /// <summary>
        /// The Entrance of the program.
        /// </summary>
        /// <param name="args">The arguments provided when it starts.</param>
        [STAThread]//The mode of the thread running.
        public static void Main(string[] args)
        {
            Application.Run(new GoBoard(19));
        }
    }
}
