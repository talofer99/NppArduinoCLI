﻿using Kbg.NppPluginNET.PluginInfrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace Kbg.NppPluginNET
{
    public class BoardsRoot
    {
        public List<Board> Boards { get; set; }
        public List<Port> Ports { get; set; }
    }

    public class Board
    {
        public string name { get; set; }
        public string FQBN { get; set; }
        //public string address { get; set; }
    }

    public class Port
    {
        public string address { get; set; }
        public string protocol { get; set; }
        public string protocol_label { get; set; }
        public Board boards { get; set; }

    }



    public partial class frmMyDlg : Form
    {
        private readonly IScintillaGateway editor;
        public List<Port> ConnectedBoards { get; set; } = new List<Port>();
        public List<Board> InstalledBoards { get; set; } = new List<Board>();

        // init
        public frmMyDlg(IScintillaGateway editor)
        {
            this.editor = editor;
            InitializeComponent();

            // get the connected board list
            GetConnectedBoardsList();
            // set the board list in cimbo box
            //SetConnecteBoardsComboBox();

            // get the installed list 
            GetInstalledBoardsList();
            //set the list in the drop down 
            SetInstalledBoardsComboBox();
        }




        private void SetBoardConfig(int index)
        {
            // now lest check othe boards properties
            string getBoardResult = RunCLICommand("board details " + InstalledBoards[index].FQBN);
            RichTextBox1.Text = getBoardResult;


        } //end private void SetBoardConfig


        // CLI UPLOAD PROCESS
        private Boolean uploadSketch(string targetINOPath)
        {

            // get selected board.
            int dropDownIndex = comboBox2.SelectedIndex;
            // get the right board 
            Board CompileBoard = InstalledBoards[dropDownIndex];
            // get selected board.
            string selectedBoard = (string)comboBox1.SelectedItem;
            // get the right board 
            Port runOnThisBoard = getConnectedBoard_ByName(selectedBoard);
            // now lets create the CLI command 
            string CLICommand = "upload  -b " + CompileBoard.FQBN + " -p " + runOnThisBoard.address + " " + targetINOPath;

            // run it           
            string uploadResult = RunCLICommand(CLICommand);
            //set the result out
            RichTextBox1.Text = uploadResult;
            // return is process ok
            return IsProccessOk(uploadResult);

        } //end private Boolean uploadSketch

        // CLI COMPILE PROCESS
        private Boolean compileSketch(string targetINOPath)
        {
            // get selected board.
            int dropDownIndex = comboBox2.SelectedIndex;
            // get the right board 
            Board CompileBoard = InstalledBoards[dropDownIndex];
            // now lets create the CLI command 
            string CLICommand = "compile -b " + CompileBoard.FQBN + " " + targetINOPath;
            // run it           
            string compiledResult = RunCLICommand(CLICommand);
            //set the result out
            RichTextBox1.Text = compiledResult;
            // return is process ok
            return IsProccessOk(compiledResult);
        } //end private Boolean compileSketch

        // get the list of connected boards (COM PORTS)
        private void GetConnectedBoardsList()
        {
            // get board list
            string getBoardResult = RunCLICommand("board list");

            // clear the list 
            ConnectedBoards.Clear();
            RichTextBox1.Text = getBoardResult;
            JavaScriptSerializer js = new JavaScriptSerializer();
            var boards = js.Deserialize<BoardsRoot>(getBoardResult);
            if (boards != null && boards.Ports != null && boards.Ports.Any())
            {
                InstalledBoards.AddRange(boards.Ports);
            } //end if 
        } //end private void GetConnectedBoardsList



        // get list of installed type of boards 
        private void GetInstalledBoardsList()
        {
            // get board list
            string getBoardResult = RunCLICommand("board listall");

            InstalledBoards.Clear();

            JavaScriptSerializer js = new JavaScriptSerializer();
            var boards = js.Deserialize<BoardsRoot>(getBoardResult);

            if (boards != null && boards.Boards != null && boards.Boards.Any())
            {
                InstalledBoards.AddRange(boards.Boards);
            }// end if 

            // sort list by FQBN
            InstalledBoards.Sort((x, y) => x.FQBN.CompareTo(y.FQBN));

        } //end  private void GetInstalledBoardsList




        // runs the CLI command and return a string of the JSON
        string RunCLICommand(string arguments)
        {
            //Create process
            System.Diagnostics.Process pProcess = new System.Diagnostics.Process();
            //strCommand is path and file name of command to run
            pProcess.StartInfo.FileName = "arduino-cli";
            //strCommandParameters are parameters to pass to program
            pProcess.StartInfo.Arguments = arguments + " --format json";
            pProcess.StartInfo.UseShellExecute = false;
            //Set output of program to be written to process output stream
            pProcess.StartInfo.RedirectStandardOutput = true;
            //Optional
            //pProcess.StartInfo.WorkingDirectory = "C:\";
            //Start the process
            pProcess.Start();
            //Get program output
            string strOutput = pProcess.StandardOutput.ReadToEnd();
            //Wait for process to finish
            pProcess.WaitForExit();
            return strOutput;

        } //end string RunCLICommand




        /***********************************************************
        * UI RELATED 
        ***********************************************************/

        // button1 - REFRESH BUTTON CLICK FUNCTION
        private void button1_Click(object sender, EventArgs e)
        {
            // get the connected board list
            GetConnectedBoardsList();
            // set the board list in cimbo box
            SetConnecteBoardsComboBox();

            // get the installed list 
            //GetInstalledBoardsList();
            //set the list in the drop down 
            //SetInstalledBoardsComboBox();

        } // end private void button1_Click

        // button2 - COMPILE BUTTON CLICK FUNCTION
        private void button2_Click(object sender, EventArgs e)
        {
            // we first make sure the file ext is ino
            if (!isTargetFileINO())
                return;

            // clear otuput text box 
            RichTextBox1.Text = "";
            // compile sketch
            Boolean isCompiled = compileSketch(getTargetPath());

        } //end private void button2_Click

        // button3 - UPLOAD BUTTON CLICK FUNCTION
        private void button3_Click(object sender, EventArgs e)
        {
            // we first make sure the file ext is ino
            if (!isTargetFileINO())
                return;

            // clear otuput text box 
            RichTextBox1.Text = "";
            // compile sketch
            Boolean isCompiled = compileSketch(getTargetPath());
            //if compiled
            if (isCompiled)
            {
                // upload
                Boolean isUploaded = uploadSketch(getTargetPath());
                // if uploaded 
                if (isUploaded)
                    // update output text box
                    RichTextBox1.Text = "Upload completed!";
            } //end if 

        } //end private void button3_Click

        // comboBox1 - COM PORT - FUNCTION GET TRIGGERED ON ANY CHANGE INCLUDING DELETE !!
        private void comboBox1_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            //ComboBox comboBox = (ComboBox)sender;
            int selectComboIndex = comboBox1.SelectedIndex;
            // if no com port is selected 
            if (selectComboIndex == -1)
            {
                // disable upload 
                button3.Enabled = false;
            }
            else
            {
                // if board have FQBN
                if (ConnectedBoards[selectComboIndex].boards.FQBN != "-1")
                {
                    // find it on the list
                    int getBoardID = getInstalledBoardID_ByFQBN(ConnectedBoards[selectComboIndex].boards.FQBN);
                    // if found 
                    if (getBoardID != -1)
                        // select in the list
                        comboBox2.SelectedIndex = getBoardID;
                } //end if 

                // only set uplaod to true if the ocombobox2 is slected as well
                if (comboBox2.SelectedIndex != -1)
                    button3.Enabled = true;
            } //end if 
        } // end private void comboBox1_SelectedIndexChanged


        // comboBox2 - BOARD TYPE - FUNCTION GET TRIGGERED ON ANY CHANGE INCLUDING DELETE !!
        private void comboBox2_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            //ComboBox comboBox = (ComboBox)sender;
            // if none is selcted 
            if (comboBox2.SelectedIndex == -1)
            {
                // disable compile and upload
                button2.Enabled = false;
                button3.Enabled = false;
            }
            else
            {
                // enable compile and upload
                button2.Enabled = true;
                // make sure we got com port selected
                if (comboBox1.SelectedIndex != -1)
                    button3.Enabled = true;
                // set the board cofig in the UI
                //SetBoardConfig(selectComboIndex);

            } //end if 
        } //end private void comboBox2_SelectedIndexChanged

        // set boards in combo box
        private void SetInstalledBoardsComboBox()
        {
            // clear the drop down and refill 
            comboBox2.Items.Clear();
            foreach (Board InstalleddBoard in InstalledBoards)
            {
                comboBox2.Items.Add(InstalleddBoard.name);
            } //end foreach
        } // end private void SetInstalledBoardsComboBox()

        // set boards in combo box
        private void SetConnecteBoardsComboBox()
        {
            // clear the drop down and refill 
            comboBox1.Items.Clear();
            foreach (Port ConnectedBoard in ConnectedBoards)
            {
                string boardDisply = ConnectedBoard.address;
                // only if we know the type 
                if (ConnectedBoard.boards.FQBN != "-1")
                {
                    boardDisply += " - " + ConnectedBoard.boards.name;
                } //end if 

                comboBox1.Items.Add(boardDisply);
            } //end foreach
        } //end private void SetConnecteBoardsComboBox()


        /***********************************************************
        * UTIL
        ***********************************************************/

        // IS OK PROCESS - is contain ERROR its not
        private Boolean IsProccessOk(string processOutput)
        {
            if (processOutput.Contains("Error"))
                return false;

            return true;
        } //end private Boolean IsProccessOk

       

        // search connected boards by display name
        private Port getConnectedBoard_ByName(string name)
        {
            foreach (Port ConnectedBoard in ConnectedBoards)
            {
                if (name.Contains(ConnectedBoard.address))
                {
                    return ConnectedBoard;
                }
            } //end for
            return new Port();
        } //end private Board getConnectedBoard_ByName

        // get installed board id by FQBN
        private int getInstalledBoardID_ByFQBN(string FQBN)
        {
            int counter = 0;
            foreach (Board InstalleddBoard in InstalledBoards)
            {
                if (InstalleddBoard.FQBN == FQBN)
                {
                    return counter;
                }
                counter++;
            } //end for
            return -1;
        } //end private int getInstalledBoardID_ByFQBN


        // get target path from NPP
        private string getTargetPath()
        {
            // get path
            StringBuilder path = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_GETCURRENTDIRECTORY, 0, path);

            return path.ToString();
        } //end private string getTargetPath


        // check if file is INO
        Boolean isTargetFileINO()
        {
            // get file name 
            StringBuilder filename = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(PluginBase.nppData._nppHandle, (uint)NppMsg.NPPM_GETFILENAME, 0, filename);
            // check if INO
            if (filename.ToString().Substring(filename.ToString().Length - 3, 3) != "ino")
                return false;
            return true;

        } //end Boolean isTargetFileINO

    }
}