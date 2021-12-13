using System;
using Terminal.Gui;
using Grpc.Net.Client;
using System.Diagnostics;

namespace gRPCDemoClient
{
    public class MainWindow
    {
        private readonly Toplevel _top;
        private Window? _window;
        private MenuBar? _menuBar;
        private FrameView? _questsListFrame;
        private ListView? _questsListView;
        private FrameView? _questDetailFrame;
        private FrameView? _infoFrame;
        private ListView? _infoListView;

        private Label? _documentIdLabel;
        private Label? _nameLabel;
        private Label? _descriptionLabel;
        private Label? _pointsLabel;
        private Label? _pointTypeLabel;

        private readonly GrpcChannel _channel;

        private List<Quest> _quests;

        public MainWindow() 
        {
            _top = Application.Top;
            _channel = GrpcChannel.ForAddress("http://localhost:6000");

            _quests = new List<Quest>();

            InitControls();
            InitStyle();
        }

        public void InitControls()
        {
            _window = new Window("gRPC Quest Demo")
            {
                X = 0,
                Y = 1,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            InitMenu();
            InitListFrame();
            InitDetailFrame();
            InitInfoFrame();

            _top.Add(_window);
            _top.Add(_menuBar);

            _window.Add(_questsListFrame);
            _window.Add(_questDetailFrame);
            _window.Add(_infoFrame);
        }

        public void InitStyle()
        {
	    }
        
        private void InitListFrame()
        {
            _questsListFrame = new FrameView("Quests")
            {
                X = 0,
                Y = 0, //for menu
                Width = Dim.Percent(30, true), 
                Height = Dim.Percent(80, true),
                CanFocus = false,
            };
            _questsListView = new ListView()
            {
                X = 1,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                AllowsMarking = false,
                CanFocus = true
            };

            _questsListFrame.Add(_questsListView);

            //deal with opening item
            _questsListView.OpenSelectedItem += (a) =>
            {
                UpdateDetailFrame(a.Item);
            };

            Task.Run(async () => await GetQuests());
	    }

        private void InitDetailFrame()
        {
            var colorScheme = new ColorScheme() { Normal = new Terminal.Gui.Attribute(foreground: Color.DarkGray, background: Color.Blue) };

            _questDetailFrame = new FrameView("Quest Details")
            {
                X = Pos.Right(_questsListFrame),
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Percent(80, true), 
                CanFocus = true
            };

            var documentIdLabel = new Label(1, 1, "DocumentId")
            {
                ColorScheme = colorScheme
            };
            _documentIdLabel = new Label("")
            {
                X = Pos.Left(documentIdLabel),
                Y = Pos.Top(documentIdLabel) + 1,
                Width = Dim.Fill(),
            };
            _questDetailFrame.Add(documentIdLabel);
            _questDetailFrame.Add(_documentIdLabel);

            var nameLabel = new Label("Name")
            {
                X = 1,
                Y = Pos.Top(_documentIdLabel) + 2,
                ColorScheme = colorScheme
            };
            _nameLabel = new Label("")
            {
                X = Pos.Left(nameLabel),
                Y = Pos.Top(nameLabel) + 1,
                Width = Dim.Fill(),
                Height = 2,
            };
            _questDetailFrame.Add(nameLabel);
            _questDetailFrame.Add(_nameLabel);

            var pointTypeLabel = new Label("Point Type")
            {
                X = 1,
                Y = Pos.Top(_nameLabel) + 2,
                ColorScheme = colorScheme
            };
            _pointTypeLabel = new Label("")
            {
                X = Pos.Left(pointTypeLabel),
                Y = Pos.Top(pointTypeLabel) + 1,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                TextDirection = TextDirection.LeftRight_TopBottom
            };
            _questDetailFrame.Add(pointTypeLabel);
            _questDetailFrame.Add(_pointTypeLabel);

            var pointsLabel = new Label("Reward Points")
            {
                X = 1,
                Y = Pos.Top(_pointTypeLabel) + 2,
                ColorScheme = colorScheme
            };
            _pointsLabel = new Label("")
            {
                X = Pos.Left(pointsLabel),
                Y = Pos.Top(pointsLabel) + 1,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                TextDirection = TextDirection.LeftRight_TopBottom
            };
            _questDetailFrame.Add(pointsLabel);
            _questDetailFrame.Add(_pointsLabel);

            var descriptionLabel = new Label("Description")
            {
                X = 1,
                Y = Pos.Top(_pointsLabel) + 2,
                ColorScheme = colorScheme
            };
            _descriptionLabel = new Label("")
            {
                X = Pos.Left(descriptionLabel),
                Y = Pos.Top(descriptionLabel) + 1,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                TextDirection = TextDirection.LeftRight_TopBottom
            };
            _questDetailFrame.Add(descriptionLabel);
            _questDetailFrame.Add(_descriptionLabel);

        }

        private void InitInfoFrame()
        {
            _infoFrame = new FrameView("Information")
            {
                X = 0,
                Y = Pos.Bottom(_questsListFrame),
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                CanFocus = true 
            };

            _infoListView = new ListView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                AllowsMarking = false,
                CanFocus = true
            };
            _infoFrame.Add(_infoListView);
        }

        private void InitMenu()
        {
            //setup menu
            _menuBar = new MenuBar(new MenuBarItem[]
            {
                new MenuBarItem("_File", new MenuItem[]
                {
                    new MenuItem("_Quit", "", () => Application.RequestStop())
                }),
                new MenuBarItem("_Add", new MenuItem[]
		        {
                    new MenuItem("_Mock Quests", "", () => {
                    {
                        Task.Run(async () => await AddQuests());
                    } }, null, null, Key.F7)
		        }),
                new MenuBarItem("_Refresh", "", () => {
                    Task.Run(async() => await GetQuests());
                })
            });
        }
        private async Task AddQuests() 
	    { 
            try
            {
                var questClient = new QuestService.QuestServiceClient(_channel);
                var quests = GetMockQuests();
                foreach (var quest in quests) 
		        {
                    var result = await questClient.SetQuestAsync(quest, deadline: DateTime.UtcNow.AddSeconds(5));
		        }
            }
            catch(Exception ex) 
	        {
                ShowErrorMessageBox(ex);
	        }
	    }

        private async Task GetQuests() 
	    {
            try
            {
                var infoList = new List<string>();
                var questList = new List<string>();
                var questClient = new QuestService.QuestServiceClient(_channel);
                var stopWatch = new Stopwatch();

                stopWatch.Start();

                /* ***********
                 * ** call gRPC Service and get data
                 * ***********
		         */
                var questsResult = await questClient.GetQuestsAsync(new QuestsRequest { ActiveOnly = true }, deadline: DateTime.UtcNow.AddSeconds(5));

                stopWatch.Stop();
                infoList.Add($"Request Time: {stopWatch.Elapsed.Milliseconds} ms");
                if (questsResult != null) 
		        {
                    if (!string.IsNullOrEmpty(questsResult.ExecutionTime))
                    {
                        infoList.Add($"Database Execution Time: {questsResult.ExecutionTime}");
                        infoList.Add($"Database Elapsed Time: {questsResult.ElapsedTime}");
                    }
                    //todo add items to the list
                    _quests.Clear();
                    foreach (var quest in questsResult.Quests_) 
		            {
                        _quests.Add(quest);
                        questList.Add($"{quest.Name}");
		            }
                    if (questList.Any()) 
		            {
                        UpdateDetailFrame(0);
                        _questsListView?.SetSource(questList);
                        _questsListView?.SetFocus();
		            }
		        }
                _infoListView?.SetSource(infoList);
                _infoListView?.SetFocus();
                
             }
            catch (Exception ex) 
	        {
                ShowErrorMessageBox(ex);
	        }
	    }

        private void UpdateDetailFrame(int item)
        {
            var quest = _quests[item];

            if (_documentIdLabel is not null 
		        && _nameLabel is not null 
		        && _descriptionLabel is not null
		        && _pointsLabel is not null 
		        && _pointTypeLabel is not null)
            {
                _documentIdLabel.Text = quest.DocumentId;
                _documentIdLabel.SetFocus();

                _nameLabel.Text = quest.Name;
                _nameLabel.SetFocus();

                _descriptionLabel.Text = quest.Description;
                _descriptionLabel.SetFocus();

                _pointTypeLabel.Text = quest.RewardPointType;
                _pointTypeLabel.SetFocus();

                _pointsLabel.Text = quest.RewardPoints.ToString();
                _pointsLabel.SetFocus();
            }
        }

        private void ShowErrorMessageBox(Exception ex)
        {
            var errorList = new List<string> { ex.Message };
            if (ex.StackTrace is not null)
                errorList.Add(ex.StackTrace);

            _infoListView?.SetSource(errorList);
        }

        private IEnumerable<Quest> GetMockQuests() 
	    {
            var quests = new List<Quest>();

            quests.Add(new Quest { DocumentId = "dec061b9-b973-48d4-9971-44c1dc536c9f", Name = "A cure for the town", Description = "A recent outbreak of some weird disease has caused the townsfolk to go insane. Unfortunately we have no cure and their disease will lead to a painful death. Hero, I know this is a terrible task to ask of you, but we need you to end their suffering. Know that we found who's responsible, we'll get those vulgar barbarians. I wish I could join you, but alas I cannot. Know that I've put all my faith in you, you will succeed. I know it.", DocumentType="quest", IsActive = true, RewardPoints = 100, RewardPointType = "XP" });

            quests.Add(new Quest { DocumentId = "256847b3-a780-45bf-b28f-af48a309b254", Name = "Defeat the savages", Description = "Times are dire, very dire indeed. Our town has been surrounded by small groups of savages and it's just a matter of time before they attack. Fortunately they haven't formed a big group yet, so they should be easier to deal with. Hero, please get rid of those vicious gnomes. Unfortunately I cannot join you, but I have faith in you, I know you'll succeed.  I doubt you'll have problems with the gnomes. Kill as many of them as you'd like, just make sure you don't forget your main goal.", DocumentType = "quest", IsActive = true, RewardPoints = 500, RewardPointType = "Gold" });

            return quests;
	    }
    }
}

