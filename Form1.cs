using MusicVisualizer.Audio;
using MusicVisualizer.Visualization;
using MusicVisualizer.Metadata;

namespace MusicVisualizer;

public partial class Form1 : Form
{
    private AudioPlayer _player = new AudioPlayer();
    private Label _timeLabel = new Label();
    private System.Windows.Forms.Timer _timer = new System.Windows.Forms.Timer();
    private TrackBar _progressBar = new TrackBar();
    private bool _isDragging = false;
    private Label _audioLevelLabel = new Label();
    private Label _spectrumLabel = new Label();
    private Label _bandsLabel = new Label();
    private CircleVisualizer _visualizer = new CircleVisualizer();
    private LiveAudioCapture? _liveCapture;
    private Button? _mp3Tab;
    private Button? _liveTab;
    private AudioAnalyzer? _liveAnalyzer;
    private long _liveBytes = 0;
    private float _livePeak = 0f;
    private TrackInfo? _currentTrack;
    private Label _trackTitleLabel = new Label();
    private Label _trackArtistLabel = new Label();
    private System.Windows.Forms.Timer _metadataTimer = new();
    private bool _isReadingMetadata = false;
    private bool _isFullscreen = false;
    private Rectangle _previousBounds;
    private FormWindowState _previousWindowState;
    private FormBorderStyle _previousBorderStyle;
        public Form1()
    {
        InitializeComponent();
        Text = "Music Visualizer";
        Button openButton = new Button();
        openButton.Text = "Open MP3";
        openButton.SetBounds(20,20,150,40);
        openButton.Click += OpenButton_Click;
        Controls.Add(openButton);
        Button playButton = new Button();
        playButton.Text = "Play";
        playButton.SetBounds(20, 80, 100, 40);
        playButton.Click += PlayButton_Click;
        Controls.Add(playButton);
        Button pauseButton = new Button();
        pauseButton.Text = "Pause";
        pauseButton.SetBounds(140, 80, 100, 40);
        pauseButton.Click += PauseButton_Click;
        Controls.Add(pauseButton);
        Button stopButton = new Button();
        stopButton.Text = "Stop";
        stopButton.SetBounds(260, 80, 100, 40);
        stopButton.Click += StopButton_Click;
        Controls.Add(stopButton);
        FormClosed += Form1_FormClosed;
        _timeLabel.Text = "00:00 / 00:00";
        _timeLabel.SetBounds(20, 150, 250, 40);
        _timeLabel.Font = new Font("Segoe UI", 14);
        Controls.Add(_timeLabel);
        _progressBar.SetBounds(20, 200, 350, 45);
        _progressBar.Minimum = 0;
        _progressBar.Maximum = 1;
        _progressBar.TickStyle = TickStyle.None;
        _progressBar.Scroll += ProgressBar_Scroll;
        _progressBar.MouseDown += ProgressBar_MouseDown;
        _progressBar.MouseUp += ProgressBar_MouseUp;
        Controls.Add(_progressBar);
        _audioLevelLabel.Text = "Audio Level: 0.000";
        _audioLevelLabel.SetBounds(20, 260, 300, 30);
        _audioLevelLabel.Font = new Font("Segoe UI", 14);
        Controls.Add(_audioLevelLabel);
        _spectrumLabel.Text = "Spectrum: Bass: 0 / Treble: 0";
        _spectrumLabel.SetBounds(20, 300, 400, 30);
        Controls.Add(_spectrumLabel);
        _bandsLabel.Text = "Bands: 0 / 0 / 0";
        _bandsLabel.SetBounds(20, 340, 390, 30);
        Controls.Add(_bandsLabel);
        _visualizer.SetBounds(430, 50, 320, 320);
        Controls.Add(_visualizer);
        BackColor = Color.FromArgb(12, 16, 30);
        ClientSize = new Size(900, 640);
        openButton.Visible = false;
        playButton.Visible = false;
        pauseButton.Visible = false;
        stopButton.Visible = false;
        _timeLabel.Visible = false;
        _progressBar.Visible = false;
        _audioLevelLabel.Visible = false;
        _spectrumLabel.Visible = false;
        _bandsLabel.Visible = false;
       _visualizer.Size = new Size(480, 480);
       _visualizer.Location = new Point((ClientSize.Width - _visualizer.Width) / 2, (ClientSize.Height - _visualizer.Height) / 2);
       Resize += (_, __) =>
       {
           _visualizer.Location = new Point((ClientSize.Width - _visualizer.Width) / 2, (ClientSize.Height - _visualizer.Height) / 2);
         PositionTrackLabels();
       };
       Button mp3Tab = new Button()
        {
            Text = "MP3",
            Location = new Point(18, 16),
            Size = new Size(75, 30),
            BackColor = Color.FromArgb(26, 48, 63),
            ForeColor = Color.Cyan,
            FlatStyle = FlatStyle.Flat
        };
        mp3Tab.FlatAppearance.BorderSize = 0;
        mp3Tab.Cursor = Cursors.Hand;
        mp3Tab.FlatAppearance.MouseOverBackColor = Color.FromArgb(36, 65, 80);
        mp3Tab.FlatAppearance.MouseDownBackColor = Color.FromArgb(18, 92, 103);
        mp3Tab.Click += (_, __) => OpenButton_Click(this, EventArgs.Empty);
        Controls.Add(mp3Tab);
        _mp3Tab = mp3Tab;
        _mp3Tab.Visible = false;

        Button liveTab = new Button()
        {
            Text = "Live",
            Location = new Point(99, 16),
            Size = new Size(90, 30),
            BackColor = Color.FromArgb(12, 16, 30),
            ForeColor = Color.LightGray,
            FlatStyle = FlatStyle.Flat,
            Enabled = true
        };
        liveTab.FlatAppearance.BorderSize = 0;
        liveTab.Cursor = Cursors.Hand;
        liveTab.FlatAppearance.MouseOverBackColor = Color.FromArgb(36, 65, 80);
        liveTab.FlatAppearance.MouseDownBackColor = Color.FromArgb(18, 92, 103);
        async Task StartLiveAsync() 
        {
            if (_liveCapture != null)
            {
                return;
            }
            _player.Pause();
            _liveCapture = new LiveAudioCapture();
            _liveAnalyzer = new AudioAnalyzer(_liveCapture.WaveFormat);
            int liveChannels = _liveCapture.WaveFormat.Channels;
            _liveCapture.DataAvailable += (_, e) =>
            {
                System.Threading.Interlocked.Add(ref _liveBytes, e.BytesRecorded);
                _liveAnalyzer?.FeedLiveSamples(e.Buffer, e.BytesRecorded, liveChannels);
                float peak = 0f;
                for (int i =0; i + 4 <= e.BytesRecorded; i += 4)
                {
                    float sample = BitConverter.ToSingle(e.Buffer, i);
                    peak = Math.Max(peak, Math.Abs(sample));
                }
                System.Threading.Volatile.Write(ref _livePeak, peak);
            };

            _liveCapture.Start();
            UpdateTabColors(true);
            _currentTrack = null;
            _visualizer.setCoverArt(null);
            UpdateTrackLabels();

            await RefreshLiveTrackAsync();
        }
        liveTab.Click += async (_, __) => await StartLiveAsync();

    Controls.Add(liveTab);
    _liveTab = liveTab;
    _liveTab.Visible = false;
    _trackTitleLabel.Font = new Font("Segoe UI", 18f, FontStyle.Bold);
    _trackTitleLabel.ForeColor = Color.White;
    _trackTitleLabel.TextAlign = ContentAlignment.MiddleCenter;
    _trackTitleLabel.AutoEllipsis = true;
    _trackTitleLabel.Text = "";
    Controls.Add(_trackTitleLabel);
    _trackArtistLabel.Font = new Font("Segoe UI", 11f);
    _trackArtistLabel.ForeColor = Color.FromArgb(145, 160, 190);
    _trackArtistLabel.TextAlign = ContentAlignment.MiddleCenter;
    _trackArtistLabel.AutoEllipsis = true;
    _trackArtistLabel.Text = "";
    Controls.Add(_trackArtistLabel);
    PositionTrackLabels();




       
        _timer.Interval = 16;
        _timer.Tick += Timer_Tick;
        _timer.Start(); 
        _metadataTimer.Interval = 1000;
        _metadataTimer.Tick += async (_, __) => await RefreshLiveTrackAsync();
        _metadataTimer.Start();
        Shown += async (_, __) =>
        {
            await StartLiveAsync();
        };
    }
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.F11)
        {
            ToggleFullscreen();
            return true;
        }
        if (keyData == Keys.Escape && _isFullscreen)
        {
            ToggleFullscreen();
            return true;
        }

        if (keyData == Keys.Space && _player.IsLoaded)
        {
            if (_player.playbackState == NAudio.Wave.PlaybackState.Playing)
            {
                _player.Pause();
        
            }
            else
            {
                _player.Play();
            }
            return true;
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }
        private void OpenButton_Click(object sender, EventArgs e)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = "MP3 files (*.mp3)|*.mp3|All files (*.*)|*.*";
        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            try
            {
                _player.Load(openFileDialog.FileName);
                _liveCapture?.Stop();
                _liveCapture?.Dispose();
                _liveCapture = null;
                _liveAnalyzer = null;
                _progressBar.Maximum = Math.Max(1, (int)(_player.Duration.TotalSeconds*10));
                _progressBar.Value = 0;
                _player.Play();
                UpdateTabColors(false);
                try
                {
                    _currentTrack = Mp3MetadataReader.Read(openFileDialog.FileName);
                    _visualizer.setCoverArt(_currentTrack.CoverArt);
                    UpdateTrackLabels();
                }
                catch (Exception ex)
                {
                    _currentTrack = null;
                    _visualizer.setCoverArt(null);
                    UpdateTrackLabels();
                    MessageBox.Show($"Error reading metadata: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading audio file: {ex.Message}");
            }
        }
        openFileDialog.Dispose();
    }

    private void PlayButton_Click(object sender, EventArgs e)
    {
        
        _player.Play();
    }

    private void PauseButton_Click(object sender, EventArgs e)
    {
        _player.Pause();
    }
    private void StopButton_Click(object sender, EventArgs e)
    {
        _player.Stop(); 
    }
     private void Form1_FormClosed(object sender, FormClosedEventArgs e)
    {
        _player.Dispose();
        _timer.Stop();
        _timer.Dispose();
        _liveCapture?.Dispose();
        
        
    }
    private void Timer_Tick(object sender, EventArgs e)
    {

   
        if (_liveCapture != null && _liveAnalyzer != null)
        {
            float[] liveSamples = _liveAnalyzer.GetSamples();
            float livesumSquares = 0f;
           foreach (float sample in liveSamples)
            {
                livesumSquares += sample * sample;
            }
            float liveRms = MathF.Sqrt(livesumSquares / liveSamples.Length);
            float[] liveSpectrum = _liveAnalyzer.GetSpectrum(); 
            float liveBass = 0f;
            for (int i = 2; i <= 12; i++)
            {
                liveBass += liveSpectrum[i];
            }
            liveBass /= 11;
            float[] liveBands = _liveAnalyzer.GetBands();
            _visualizer.SetBands(liveBands, liveBass, liveRms);
            return;
        }
        if (!_player.IsLoaded)
        {
            return;
        }
            string current = _player.CurrentTime.ToString(@"mm\:ss");
            string duration = _player.Duration.ToString(@"mm\:ss");
            _timeLabel.Text = $"{current} / {duration}";
            if (!_isDragging)
            {
            _progressBar.Value = Math.Min(_progressBar.Maximum, (int)_player.CurrentTime.TotalSeconds * 10);
            }
            float peak = 0f;
            float[] samples = _player.GetSamples();
            float sumSquares = 0f;

            foreach (float sample in samples)
            {
                peak = Math.Max(peak, Math.Abs(sample));
                sumSquares += sample * sample;
            }
            float rms = samples.Length == 0 ? 0f : MathF.Sqrt(sumSquares / samples.Length);
            _audioLevelLabel.Text = $"Peak: {peak:F3} | RMS: {rms:F3}";

            float[] spectrum = _player.GetSpectrum();
            if (spectrum.Length == 0)
        {
            return;
        }
        float bass = 0;
        float treble = 0;

        for (int i = 2; i <= 12; i++)
        {
            bass += spectrum[i];
        }

        bass /= 11;

        for (int i = 100; i < spectrum.Length; i++)
        {
            treble += spectrum[i];
        }
        treble /= (spectrum.Length - 100);
        _spectrumLabel.Text = $"Spectrum: Bass: {bass:F6} / Treble: {treble:F6}";

        float[] bands = _player.GetBands();
        if (bands.Length == 64)
        {
            _bandsLabel.Text = $"Bands: {bands[0]:F6} / {bands[8]:F6} / {bands[16]:F6} / {bands[24]:F6}";
            _visualizer.SetBands(bands, bass, rms); 
        }
    }
    private void ProgressBar_Scroll(object sender, EventArgs e)
    {
        if (!_player.IsLoaded)
        {
            return;
        }
            _player.CurrentTime = TimeSpan.FromSeconds(_progressBar.Value / 10.0);
        
    }
    private void ProgressBar_MouseDown(object sender, MouseEventArgs e)
    {
        _isDragging = true;
    }
    private void ProgressBar_MouseUp(object sender, MouseEventArgs e)
    {
        _isDragging = false;

        if (_player.IsLoaded)
        {
            _player.CurrentTime = TimeSpan.FromSeconds(_progressBar.Value / 10.0);
        }
    }
    private void UpdateTabColors(bool isLive)
    {
        if (_mp3Tab != null && _liveTab != null)
        {
            Color active = Color.FromArgb(26, 48, 63);
            Color inactive = Color.FromArgb(12, 16, 30);
            _mp3Tab.BackColor = isLive ? inactive : active;
            _mp3Tab.ForeColor = isLive ? Color.LightGray : Color.Cyan;
            _liveTab.BackColor = isLive ? active : inactive;
            _liveTab.ForeColor = isLive ? Color.Cyan : Color.LightGray;
        }
    }

    private void PositionTrackLabels()
    {
        _trackTitleLabel.SetBounds(_visualizer.Left, _visualizer.Bottom - 10, _visualizer.Width, 35);
        _trackArtistLabel.SetBounds(_visualizer.Left, _visualizer.Bottom + 25, _visualizer.Width, 25);
        _trackTitleLabel.BringToFront();
        _trackArtistLabel.BringToFront();
    }

    private void UpdateTrackLabels()
    {
        _trackTitleLabel.Text = _currentTrack?.Title ?? "";
        _trackArtistLabel.Text = _currentTrack?.Artist ?? "";
    }


    private async Task RefreshLiveTrackAsync()
    {
        if (_liveCapture == null || _isReadingMetadata)
        {
            return;
        }
        var capture = _liveCapture;
        _isReadingMetadata = true;
        try
        {
            var track = await WindowsMediaReader.ReadAsync();
            if (capture != _liveCapture)
            {
                return;
            }
            _currentTrack = track;
            UpdateTrackLabels();
            _visualizer.setCoverArt(_currentTrack?.CoverArt);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error reading live metadata: {ex.Message}");
        }
        finally
        {
            _isReadingMetadata = false;
        }
        }
        private void ToggleFullscreen()
    {
        if (!_isFullscreen)
        {
            _previousWindowState = WindowState;
            _previousBounds = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;
            Rectangle screenBounds = Screen.FromControl(this).Bounds;
            _previousBorderStyle = FormBorderStyle;
            WindowState = FormWindowState.Normal;
            FormBorderStyle = FormBorderStyle.None;
            Bounds = screenBounds;
            _isFullscreen = true;
        }
        else
        {
          FormBorderStyle = _previousBorderStyle;
          Bounds = _previousBounds;
          WindowState = _previousWindowState;  
          _isFullscreen = false;
        }
        }
    }
    


