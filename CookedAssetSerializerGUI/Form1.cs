using System.Diagnostics;
using System.Runtime.InteropServices;
using CookedAssetSerializer;
using UAssetAPI;
using static CookedAssetSerializer.Globals;

namespace CookedAssetSerializerGUI;

public partial class Form1 : Form {
    public Form1() {
        InitializeComponent();
        setupForm();
    }

    private Globals settings;

    #region CustomFormSetup
    private const int HT_CAPTION = 0x2;
    private const int WM_NCLBUTTONDOWN = 0x00A1;
    [DllImport("user32", CharSet = CharSet.Auto)]
    private static extern bool ReleaseCapture();
    [DllImport("user32", CharSet = CharSet.Auto)]
    private static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

    protected override void OnMouseDown(MouseEventArgs e) {
        if (e.Button != MouseButtons.Left) return;
        Rectangle rct = DisplayRectangle;
        if (!rct.Contains(e.Location)) return;
        ReleaseCapture();
        SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
    }
    #endregion

    #region Vars
    private object[] versionOptionsKeys = {
        "Unknown version",
        "4.0",
        "4.1",
        "4.2",
        "4.3",
        "4.4",
        "4.5",
        "4.6",
        "4.7",
        "4.8",
        "4.9",
        "4.10",
        "4.11",
        "4.12",
        "4.13",
        "4.14",
        "4.15",
        "4.16",
        "4.17",
        "4.18",
        "4.19",
        "4.20",
        "4.21",
        "4.22",
        "4.23",
        "4.24",
        "4.25",
        "4.26",
        "4.27"
    };

    private UE4Version[] versionOptionsValues = {
        UE4Version.UNKNOWN,
        UE4Version.VER_UE4_0,
        UE4Version.VER_UE4_1,
        UE4Version.VER_UE4_2,
        UE4Version.VER_UE4_3,
        UE4Version.VER_UE4_4,
        UE4Version.VER_UE4_5,
        UE4Version.VER_UE4_6,
        UE4Version.VER_UE4_7,
        UE4Version.VER_UE4_8,
        UE4Version.VER_UE4_9,
        UE4Version.VER_UE4_10,
        UE4Version.VER_UE4_11,
        UE4Version.VER_UE4_12,
        UE4Version.VER_UE4_13,
        UE4Version.VER_UE4_14,
        UE4Version.VER_UE4_15,
        UE4Version.VER_UE4_16,
        UE4Version.VER_UE4_17,
        UE4Version.VER_UE4_18,
        UE4Version.VER_UE4_19,
        UE4Version.VER_UE4_20,
        UE4Version.VER_UE4_21,
        UE4Version.VER_UE4_22,
        UE4Version.VER_UE4_23,
        UE4Version.VER_UE4_24,
        UE4Version.VER_UE4_25,
        UE4Version.VER_UE4_26,
        UE4Version.VER_UE4_27,
    };
    #endregion

    private void setupForm() {
        // Temporary until saving/loading exists
        // rtxtContentDir.Text = Directory.GetCurrentDirectory();
        // rtxtJSONDir.Text = Directory.GetCurrentDirectory();
        // rtxtOutputDir.Text = Directory.GetCurrentDirectory();

        // Temporary inputs
        rtxtContentDir.Text = @"F:\DRG Modding\DRGPacker\_unpacked\FSD\Content";
        rtxtJSONDir.Text = @"F:\DRG Modding\DRGPacker\JSON";
        rtxtOutputDir.Text = @"F:\DRG Modding\DRGPacker\JSON\Output";

        cbUEVersion.Items.AddRange(versionOptionsKeys);
        cbUEVersion.SelectedIndex = 28; // This is a dumb thing to do, but oh well

        List<EAssetType> defaultAssets = new() {
            EAssetType.BlendSpaceBase,
            EAssetType.AnimSequence,
            EAssetType.SkeletalMesh,
            EAssetType.Skeleton,
            EAssetType.AnimMontage,
            EAssetType.FileMediaSource,
            EAssetType.StaticMesh,
        };
        lbAssetsToSkipSerialization.DataSource = Enum.GetValues(typeof(EAssetType));
        foreach (var asset in defaultAssets) {
            lbAssetsToSkipSerialization.SetSelected(lbAssetsToSkipSerialization.FindString(asset.ToString()), true);
        }
        // For some stupid reason, the first item in the lb is always enabled, which in this case, is the Blueprint,
        // which is the absolute worst time for this """feature""" to happen
        lbAssetsToSkipSerialization.SetSelected(0, false);
    }

    private void emptyLogFiles() {
        string[] files = {
            "debug_log.txt",
            "output_log.txt"
        };
        foreach (string file in files) {
            string path;
            if (rtxtJSONDir.Text.EndsWith("\\")) path = rtxtJSONDir.Text + file;
            else path = rtxtJSONDir.Text + "\\" + file;
            if (!File.Exists(path)) continue;
            File.Delete(path);
            outputText("Cleared log file: " + path);
        }
    }

    private string[] sanitiseInputs(string[] lines) {
        for (int i = 0; i < lines.Length; i++) {
            if (!lines[i].Contains("/Script/")) {
                lines[i] = lines[i].Insert(0, "/Script/");
            }

            // This garbage allows us to copy and paste text from the text files
            // and not have to muck about deleting them manually
            lines[i] = lines[i].Replace('"'.ToString(), "");
            lines[i] = lines[i].Replace(','.ToString(), "");
        }

        return lines;
    }

    private void setupGlobals() {
        List<string> typesToCopy = new List<string>();
        typesToCopy.AddRange(sanitiseInputs(rtxtCookedAssets.Lines));
        List<string> simpleAssets = new List<string>();
        simpleAssets.AddRange(sanitiseInputs(rtxtSimpleAssets.Lines));
        List<EAssetType> assetsToSkip = new List<EAssetType>();
        assetsToSkip.AddRange(lbAssetsToSkipSerialization.SelectedItems.Cast<EAssetType>());
        List<string> circularDependencies = new List<string>();
        circularDependencies.AddRange(sanitiseInputs(rtxtCircularDependancy.Lines));

        settings = new Globals(rtxtContentDir.Text, rtxtJSONDir.Text, rtxtOutputDir.Text,
            versionOptionsValues[cbUEVersion.SelectedIndex],
            rbRefreshAssets.Checked, assetsToSkip, circularDependencies, simpleAssets, typesToCopy);
    }

    private void disableButtons() {
        btnScanAssets.Enabled = false;
        btnMoveCookedAssets.Enabled = false;
        btnSerializeAssets.Enabled = false;
    }

    private void enableButtons() {
        btnScanAssets.Enabled = true;
        btnMoveCookedAssets.Enabled = true;
        btnSerializeAssets.Enabled = true;
    }

    private void outputText(string text) {
        if (rtxtOutput.TextLength == 0) rtxtOutput.Text += text;
        else rtxtOutput.Text += Environment.NewLine + text;
    }

    private void openFile(string path, bool bIsLog = false) {
        if (!File.Exists(path)) {
            if (!bIsLog) outputText("You need to scan the assets first!");
            return;
        }

        // I don't know why, I don't know how, but doing just Process.Start(path) doesn't fucking work,
        // even though that's the preferred option since it opens whatever editor is associated with the file extension
        Process.Start("notepad.exe", path);
    }

    private string openFileDialog() {
        FolderBrowserDialog fbd = new FolderBrowserDialog();
        fbd.ShowDialog();
        return fbd.SelectedPath;
    }

    private void btnSelectContentDir_Click(object sender, EventArgs e) {
        rtxtContentDir.Text = openFileDialog();
    }

    private void btnSelectJSONDir_Click(object sender, EventArgs e) {
        rtxtJSONDir.Text = openFileDialog();
    }

    private void btnSelectOutputDir_Click(object sender, EventArgs e) {
        rtxtOutputDir.Text = openFileDialog();
    }

    private void btnScanAssets_Click(object sender, EventArgs e) {
        setupGlobals();

        Task.Run(() => {
            disableButtons();
            try {
                settings.ScanAssetTypes();
            } catch (Exception exception) {
                // outputText("\n" + exception.ToString()); // TODO: Why the fuck does this not work lol?
                rtxtOutput.Text += Environment.NewLine + exception;
                return;
            }
            enableButtons();

            outputText("Scanned assets!");
        });
    }

    private void btnMoveCookedAssets_Click(object sender, EventArgs e) {
        setupGlobals();

        Task.Run(() => {
            disableButtons();
            try {
                settings.GetCookedAssets();
            } catch (Exception exception) {
                rtxtOutput.Text += Environment.NewLine + exception;
                return;
            }
            enableButtons();

            outputText("Moved assets!");
        });
    }

    private void btnSerializeAssets_Click(object sender, EventArgs e) {
        setupGlobals();

        Task.Run(() => {
            disableButtons();
            try {
                settings.SerializeAssets();
            } catch (Exception exception) {
                rtxtOutput.Text += Environment.NewLine + exception;
                return;
            }
            enableButtons();

            outputText("Serialized assets!");
        });
    }

    private void btnOpenAllTypes_Click(object sender, EventArgs e) {
        openFile(JSON_DIR + "\\AllTypes.txt");
    }

    private void btnOpenAssetTypes_Click(object sender, EventArgs e) {
        openFile(JSON_DIR + "\\AssetTypes.json");
    }

    private void btnOpenLogs_Click(object sender, EventArgs e) {
        openFile(JSON_DIR + "\\output_log.txt", true);
    }

    private void btnClearLogs_Click(object sender, EventArgs e) {
        emptyLogFiles();
    }
}
