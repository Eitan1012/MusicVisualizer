using MusicVisualizer.Audio;
using MusicVisualizer.Visualization;

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

        _timer.Interval = 100;
        _timer.Tick += Timer_Tick;
        _timer.Start(); 
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
                _progressBar.Maximum = Math.Max(1, (int)(_player.Duration.TotalSeconds*10));
                _progressBar.Value = 0;
                _player.Play();
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
        
        
    }
    private void Timer_Tick(object sender, EventArgs e)
    {
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
            float level = 0;
            foreach (float sample in _player.GetSamples())
            {
                level = Math.Max(level, Math.Abs(sample));
            }
            _audioLevelLabel.Text = $"Audio Level: {level:F3}";
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
            _visualizer.SetBands(bands);
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
}

