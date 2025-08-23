using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using OrganizationCalendar.Data;
using OrganizationCalendar.Models;

namespace OrganizationCalendar.Forms
{
    public partial class MainCalendarForm : Form
    {
        private readonly DatabaseConnection _database;
        private readonly EventRepository _eventRepository;
        private DateTime _currentMonth = DateTime.Today;
        private string? _selectedHouseFilter = null; // null = community view
        private List<Event> _currentEvents = new List<Event>();

        // UI Controls
        private ComboBox cmbHouseFilter = null!;
        private Button btnPreviousMonth = null!;
        private Button btnNextMonth = null!;
        private Button btnAddEvent = null!;
        private Label lblCurrentMonth = null!;
        private TableLayoutPanel calendarGrid = null!;
        private StatusStrip statusStrip = null!;
        private ToolStripStatusLabel statusLabel = null!;

        public MainCalendarForm()
        {
            _database = new DatabaseConnection();
            _eventRepository = new EventRepository(_database);
            
            InitializeComponent();
            InitializeDatabase();
            LoadCurrentMonth();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Main Form Properties
            this.Text = "Organization Calendar - SACH";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;

            // Create Menu Bar
            CreateMenuBar();

            // Create Tool Bar
            CreateToolBar();

            // Create Calendar Grid
            CreateCalendarGrid();

            // Create Status Bar
            CreateStatusBar();

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void CreateMenuBar()
        {
            var menuStrip = new MenuStrip();
            
            // File Menu
            var fileMenu = new ToolStripMenuItem("&File");
            fileMenu.DropDownItems.Add(new ToolStripMenuItem("&Import from TeamUp...", null, ImportFromTeamUp_Click));
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add(new ToolStripMenuItem("&Print Calendar...", null, PrintCalendar_Click));
            fileMenu.DropDownItems.Add(new ToolStripMenuItem("&Export Events...", null, ExportEvents_Click));
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add(new ToolStripMenuItem("E&xit", null, (s, e) => this.Close()));

            // View Menu
            var viewMenu = new ToolStripMenuItem("&View");
            viewMenu.DropDownItems.Add(new ToolStripMenuItem("&Refresh", null, RefreshCalendar_Click) { ShortcutKeys = Keys.F5 });
            viewMenu.DropDownItems.Add(new ToolStripMenuItem("&Today", null, GoToToday_Click) { ShortcutKeys = Keys.Control | Keys.T });

            // Events Menu
            var eventsMenu = new ToolStripMenuItem("&Events");
            eventsMenu.DropDownItems.Add(new ToolStripMenuItem("&Add Event...", null, AddEvent_Click) { ShortcutKeys = Keys.Control | Keys.N });
            eventsMenu.DropDownItems.Add(new ToolStripMenuItem("&Find Events...", null, FindEvents_Click) { ShortcutKeys = Keys.Control | Keys.F });

            // Help Menu
            var helpMenu = new ToolStripMenuItem("&Help");
            helpMenu.DropDownItems.Add(new ToolStripMenuItem("&About", null, About_Click));

            menuStrip.Items.AddRange(new[] { fileMenu, viewMenu, eventsMenu, helpMenu });
            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);
        }

        private void CreateToolBar()
        {
            var toolStrip = new ToolStrip();
            toolStrip.Dock = DockStyle.Top;

            // House Filter
            var lblHouse = new ToolStripLabel("View: ");
            toolStrip.Items.Add(lblHouse);

            cmbHouseFilter = new ComboBox();
            cmbHouseFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbHouseFilter.Width = 200;
            cmbHouseFilter.Items.Add("Community (All Houses)");
            foreach (var house in HouseData.DefaultHouses)
            {
                cmbHouseFilter.Items.Add(house.DisplayName);
            }
            cmbHouseFilter.SelectedIndex = 0;
            cmbHouseFilter.SelectedIndexChanged += HouseFilter_Changed;

            var houseFilterHost = new ToolStripControlHost(cmbHouseFilter);
            toolStrip.Items.Add(houseFilterHost);

            toolStrip.Items.Add(new ToolStripSeparator());

            // Navigation buttons
            btnPreviousMonth = new Button();
            btnPreviousMonth.Text = "< Previous";
            btnPreviousMonth.Click += PreviousMonth_Click;
            var prevHost = new ToolStripControlHost(btnPreviousMonth);
            toolStrip.Items.Add(prevHost);

            lblCurrentMonth = new Label();
            lblCurrentMonth.Font = new Font(lblCurrentMonth.Font, FontStyle.Bold);
            lblCurrentMonth.TextAlign = ContentAlignment.MiddleCenter;
            lblCurrentMonth.Width = 200;
            var monthHost = new ToolStripControlHost(lblCurrentMonth);
            toolStrip.Items.Add(monthHost);

            btnNextMonth = new Button();
            btnNextMonth.Text = "Next >";
            btnNextMonth.Click += NextMonth_Click;
            var nextHost = new ToolStripControlHost(btnNextMonth);
            toolStrip.Items.Add(nextHost);

            toolStrip.Items.Add(new ToolStripSeparator());

            // Add Event button
            btnAddEvent = new Button();
            btnAddEvent.Text = "Add Event";
            btnAddEvent.BackColor = Color.LightBlue;
            btnAddEvent.Click += AddEvent_Click;
            var addEventHost = new ToolStripControlHost(btnAddEvent);
            toolStrip.Items.Add(addEventHost);

            this.Controls.Add(toolStrip);
        }

        private void CreateCalendarGrid()
        {
            calendarGrid = new TableLayoutPanel();
            calendarGrid.Dock = DockStyle.Fill;
            calendarGrid.RowCount = 7; // Header + 6 weeks
            calendarGrid.ColumnCount = 7; // 7 days

            // Set equal column widths
            for (int i = 0; i < 7; i++)
            {
                calendarGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14.28f));
            }

            // Set row heights (header smaller, calendar rows larger)
            calendarGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 30)); // Header
            for (int i = 1; i < 7; i++)
            {
                calendarGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 16.67f));
            }

            // Add day headers
            string[] dayHeaders = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
            for (int i = 0; i < 7; i++)
            {
                var headerLabel = new Label();
                headerLabel.Text = dayHeaders[i];
                headerLabel.Font = new Font(headerLabel.Font, FontStyle.Bold);
                headerLabel.BackColor = Color.LightGray;
                headerLabel.TextAlign = ContentAlignment.MiddleCenter;
                headerLabel.Dock = DockStyle.Fill;
                calendarGrid.Controls.Add(headerLabel, i, 0);
            }

            this.Controls.Add(calendarGrid);
        }

        private void CreateStatusBar()
        {
            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            statusLabel.Text = "Ready";
            statusStrip.Items.Add(statusLabel);
            this.Controls.Add(statusStrip);
        }

        private void InitializeDatabase()
        {
            try
            {
                _database.InitializeDatabase();
                _database.TestConnection();
                UpdateStatusLabel("Database initialized successfully");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize database: {ex.Message}", 
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void LoadCurrentMonth()
        {
            try
            {
                // Update month label
                lblCurrentMonth.Text = _currentMonth.ToString("MMMM yyyy");

                // Get events for current month
                var monthStart = new DateTime(_currentMonth.Year, _currentMonth.Month, 1);
                var monthEnd = monthStart.AddMonths(1);

                if (_selectedHouseFilter == null)
                {
                    _currentEvents = _eventRepository.GetEventsByDateRange(monthStart, monthEnd);
                }
                else
                {
                    _currentEvents = _eventRepository.GetEventsByHouse(_selectedHouseFilter, monthStart, monthEnd);
                }

                // Clear existing calendar cells (except headers)
                var controlsToRemove = calendarGrid.Controls.Cast<Control>()
                    .Where(c => calendarGrid.GetRow(c) > 0).ToList();
                foreach (var control in controlsToRemove)
                {
                    calendarGrid.Controls.Remove(control);
                }

                // Build calendar
                BuildCalendarGrid();

                UpdateStatusLabel($"Loaded {_currentEvents.Count} events for {_currentMonth:MMMM yyyy}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading calendar: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatusLabel("Error loading calendar");
            }
        }

        private void BuildCalendarGrid()
        {
            var monthStart = new DateTime(_currentMonth.Year, _currentMonth.Month, 1);
            var firstDayOfWeek = (int)monthStart.DayOfWeek; // 0 = Sunday
            var daysInMonth = DateTime.DaysInMonth(_currentMonth.Year, _currentMonth.Month);

            int currentDay = 1;
            var today = DateTime.Today;

            for (int week = 1; week <= 6; week++) // Up to 6 weeks
            {
                for (int dayOfWeek = 0; dayOfWeek < 7; dayOfWeek++)
                {
                    var cellPanel = new Panel();
                    cellPanel.Dock = DockStyle.Fill;
                    cellPanel.BorderStyle = BorderStyle.FixedSingle;
                    cellPanel.BackColor = Color.White;

                    if (week == 1 && dayOfWeek < firstDayOfWeek)
                    {
                        // Empty cell before month starts
                        cellPanel.BackColor = Color.LightGray;
                    }
                    else if (currentDay <= daysInMonth)
                    {
                        var currentDate = new DateTime(_currentMonth.Year, _currentMonth.Month, currentDay);
                        
                        // Day number label
                        var dayLabel = new Label();
                        dayLabel.Text = currentDay.ToString();
                        dayLabel.Font = new Font(dayLabel.Font, FontStyle.Bold);
                        dayLabel.Location = new Point(2, 2);
                        dayLabel.AutoSize = true;

                        // Highlight today
                        if (currentDate == today)
                        {
                            cellPanel.BackColor = Color.LightYellow;
                            dayLabel.ForeColor = Color.Blue;
                        }

                        cellPanel.Controls.Add(dayLabel);

                        // Add events for this day
                        var dayEvents = _currentEvents.Where(e => e.StartDateTime.Date == currentDate).ToList();
                        AddEventsToCell(cellPanel, dayEvents, currentDate);

                        // Add click handler for adding events
                        cellPanel.Tag = currentDate;
                        cellPanel.Click += CalendarCell_Click;
                        cellPanel.DoubleClick += CalendarCell_DoubleClick;

                        currentDay++;
                    }
                    else
                    {
                        // Empty cell after month ends
                        cellPanel.BackColor = Color.LightGray;
                    }

                    calendarGrid.Controls.Add(cellPanel, dayOfWeek, week);
                }

                if (currentDay > daysInMonth) break;
            }
        }

        private void AddEventsToCell(Panel cell, List<Event> events, DateTime date)
        {
            const int maxEventsToShow = 3;
            int yOffset = 20;

            for (int i = 0; i < Math.Min(events.Count, maxEventsToShow); i++)
            {
                var evt = events[i];
                var eventLabel = new Label();
                eventLabel.Text = $"{evt.DisplayTime} - {evt.Title}";
                eventLabel.Location = new Point(2, yOffset);
                eventLabel.Size = new Size(cell.Width - 4, 16);
                eventLabel.Font = new Font(eventLabel.Font.FontFamily, 7);
                eventLabel.ForeColor = Color.Black;
                
                // Color coding by category
                eventLabel.BackColor = evt.Category switch
                {
                    EventCategory.Mandatory => Color.LightCoral,
                    EventCategory.Optional => Color.LightBlue,
                    EventCategory.House => Color.LightGreen,
                    _ => Color.White
                };

                eventLabel.Tag = evt;
                eventLabel.Click += EventLabel_Click;
                eventLabel.DoubleClick += EventLabel_DoubleClick;
                
                cell.Controls.Add(eventLabel);
                yOffset += 18;
            }

            // Show "more events" indicator
            if (events.Count > maxEventsToShow)
            {
                var moreLabel = new Label();
                moreLabel.Text = $"+ {events.Count - maxEventsToShow} more...";
                moreLabel.Location = new Point(2, yOffset);
                moreLabel.Size = new Size(cell.Width - 4, 16);
                moreLabel.Font = new Font(moreLabel.Font.FontFamily, 7, FontStyle.Italic);
                moreLabel.ForeColor = Color.Gray;
                moreLabel.Tag = date;
                moreLabel.Click += MoreEventsLabel_Click;
                cell.Controls.Add(moreLabel);
            }
        }

        private void UpdateStatusLabel(string message)
        {
            if (statusLabel != null)
            {
                statusLabel.Text = $"{DateTime.Now:HH:mm} - {message}";
            }
        }

        #region Event Handlers

        private void HouseFilter_Changed(object? sender, EventArgs e)
        {
            if (cmbHouseFilter.SelectedIndex == 0)
            {
                _selectedHouseFilter = null; // Community view
            }
            else
            {
                var selectedHouse = HouseData.DefaultHouses[cmbHouseFilter.SelectedIndex - 1];
                _selectedHouseFilter = selectedHouse.HouseId;
            }
            LoadCurrentMonth();
        }

        private void PreviousMonth_Click(object? sender, EventArgs e)
        {
            _currentMonth = _currentMonth.AddMonths(-1);
            LoadCurrentMonth();
        }

        private void NextMonth_Click(object? sender, EventArgs e)
        {
            _currentMonth = _currentMonth.AddMonths(1);
            LoadCurrentMonth();
        }

        private void CalendarCell_Click(object? sender, EventArgs e)
        {
            if (sender is Panel panel && panel.Tag is DateTime date)
            {
                // Single click - select date (could show day view in future)
                UpdateStatusLabel($"Selected: {date:MMMM dd, yyyy}");
            }
        }

        private void CalendarCell_DoubleClick(object? sender, EventArgs e)
        {
            if (sender is Panel panel && panel.Tag is DateTime date)
            {
                // Double click - add event for this date
                ShowAddEventDialog(date);
            }
        }

        private void EventLabel_Click(object? sender, EventArgs e)
        {
            if (sender is Label label && label.Tag is Event evt)
            {
                UpdateStatusLabel($"Selected: {evt.Title} - {evt.DisplayDateTime}");
            }
        }

        private void EventLabel_DoubleClick(object? sender, EventArgs e)
        {
            if (sender is Label label && label.Tag is Event evt)
            {
                ShowEditEventDialog(evt);
            }
        }

        private void MoreEventsLabel_Click(object? sender, EventArgs e)
        {
            if (sender is Label label && label.Tag is DateTime date)
            {
                ShowDayEventsDialog(date);
            }
        }

        private void AddEvent_Click(object? sender, EventArgs e)
        {
            ShowAddEventDialog(DateTime.Today);
        }

        private void RefreshCalendar_Click(object? sender, EventArgs e)
        {
            LoadCurrentMonth();
        }

        private void GoToToday_Click(object? sender, EventArgs e)
        {
            _currentMonth = DateTime.Today;
            LoadCurrentMonth();
        }

        private void ImportFromTeamUp_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("TeamUp import feature coming soon!", "Feature Not Available", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void PrintCalendar_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("Print calendar feature coming soon!", "Feature Not Available", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ExportEvents_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("Export events feature coming soon!", "Feature Not Available", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void FindEvents_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("Find events feature coming soon!", "Feature Not Available", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void About_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("Organization Calendar Management System\n\n" +
                "Designed for SACH Community\n" +
                "Version 1.0 MVP\n\n" +
                "Houses: AV, SP, NL, LC, WH, HH, BP", 
                "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        #region Dialog Methods

        private void ShowAddEventDialog(DateTime selectedDate)
        {
            // TODO: Implement AddEventDialog
            MessageBox.Show($"Add Event Dialog for {selectedDate:MMMM dd, yyyy}\n(Dialog coming soon!)", 
                "Add Event", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowEditEventDialog(Event eventItem)
        {
            // TODO: Implement EditEventDialog
            MessageBox.Show($"Edit Event Dialog for: {eventItem.Title}\n(Dialog coming soon!)", 
                "Edit Event", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowDayEventsDialog(DateTime date)
        {
            var dayEvents = _currentEvents.Where(e => e.StartDateTime.Date == date).ToList();
            var message = $"Events for {date:MMMM dd, yyyy}:\n\n";
            
            foreach (var evt in dayEvents)
            {
                message += $"• {evt.DisplayTime} - {evt.Title}\n";
                if (!string.IsNullOrEmpty(evt.HouseId))
                {
                    message += $"  [{evt.HouseId}] {evt.HouseName}\n";
                }
                message += "\n";
            }

            MessageBox.Show(message, $"Events - {date:MMMM dd, yyyy}", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _database?.Dispose();
            base.OnFormClosed(e);
        }
    }
}
