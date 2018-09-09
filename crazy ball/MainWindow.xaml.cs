using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Diagnostics;

namespace WPFGame1
{
    public partial class Gammer : Window
    {
       
         double RTBrick =0; 
         double RLBrick =0;
         double RRBrick =0; 
         double ballBottom =0;
         double ballLeft = 0;

        double motionRatio = 2;
        Rectangle lastCollapsed = default(Rectangle);
        int skipTick = 5;
         string[] brickInfo;
         int RedBallCurrentDirection = 0;
       int BlueBallCurrentDirection = 0;
         double RedGameBallTop = 0;
         double BlueGameBallTop = 0;
         double RedGameBallLeft = 0;
        double BlueGameBallLeft = 0;
         int currentGameState = 0;
         bool isClockWise = true; // true = clockwise , false = anti-clockwise
         static DispatcherTimer movingTimer = new DispatcherTimer();
         List<Rectangle> bricks = new List<Rectangle>();
         MediaPlayer mPlayer = new MediaPlayer();
         MediaPlayer ice = new MediaPlayer();
         MediaPlayer game_over = new MediaPlayer();

        private string[] stagesInfo;
        ImageBrush imageBackground = new ImageBrush();

        public Gammer()
        {

            InitializeComponent();
            _box.Visibility = Visibility.Hidden;
            stagesInfo = File.ReadAllLines("GameStages.txt");
            mPlayer.Open(new Uri("pay_t_his.mp3", UriKind.Relative));
            Uri ur = new Uri("ice.mp3",UriKind.Relative);
          
            ice.Open(ur);
            game_over.Open(new Uri("Game_over.mp3",UriKind.Relative));
            game_over.Position = new TimeSpan();
            ice.Position = new TimeSpan();
            ice.Volume = 1;
            game_over.Volume = 1;
            mPlayer.Volume = .5;

            RedGameBallTop = Canvas.GetTop(GameBallRed);
            BlueGameBallTop = Canvas.GetTop(GameBallBlue);
            RedGameBallLeft = Canvas.GetLeft(GameBallRed);
            BlueGameBallLeft = Canvas.GetLeft(GameBallBlue);

            movingTimer.Tick += new EventHandler(movingTimer_Tick);
            movingTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);

        }
        //   الداله دى لو الجهاز بتاعك بيدعم التاتش اسكرين 
        /*
          Action=====>Gets the last action that occurred at this location.
          Bounds=====>Gets the bounds of the area that the finger has in contact with the screen.
          Position====>Gets the location of the touch point.
          Size========>Gets the size of the Bounds property.
          TouchDevice=>Gets the touch device that generated this TouchPoint.
         */

        private void Touch_FrameReportedRed(object sender, TouchFrameEventArgs e)
        {
            if (this.canvas1 != null)
                foreach (TouchPoint _touchPoint in e.GetTouchPoints(this.canvas1))
                {
                    if (_touchPoint.Action == TouchAction.Down)
                    {
                        _touchPoint.TouchDevice.Capture(this.canvas1);
                    }
                    else if (_touchPoint.Action == TouchAction.Move && e.GetPrimaryTouchPoint(this.canvas1) != null)
                    {
                        if (_touchPoint.TouchDevice.Id == e.GetPrimaryTouchPoint(this.canvas1).TouchDevice.Id)
                        {
                            Canvas.SetLeft(rectangleRed, _touchPoint.Position.X);
                        }
                        else if (_touchPoint.TouchDevice.Id != e.GetPrimaryTouchPoint(this.canvas1).TouchDevice.Id)
                        {
                            Canvas.SetLeft(rectangleBlue, _touchPoint.Position.X);
                        }
                    }
                    else if (_touchPoint.Action == TouchAction.Up)
                    {
                        this.canvas1.ReleaseTouchCapture(_touchPoint.TouchDevice);
                    }
                }
        }


        private void changeBackgroundImage()
        {
            Random rndImage = new Random();
            int rndImageNo = rndImage.Next(1, 13);

            imageBackground.ImageSource = new BitmapImage(new Uri(string.Format("Images//img{0}.jpg", rndImageNo), UriKind.Relative));
            imageBackground.Stretch = Stretch.UniformToFill;
            myWindow.Background = imageBackground;
        }

        private void movingTimer_Tick(object sender, EventArgs e)
        {
            RedBallCurrentDirection = getDirection(GameBallRed, RedBallCurrentDirection, rectangleRed);
            BlueBallCurrentDirection = getDirection(GameBallBlue, BlueBallCurrentDirection, rectangleBlue);

            moveGameBall(RedBallCurrentDirection, ref RedGameBallTop, ref RedGameBallLeft, ref GameBallRed);
            moveGameBall(BlueBallCurrentDirection, ref BlueGameBallTop, ref BlueGameBallLeft, ref GameBallBlue);

            checkBreakCollapse(ref RedBallCurrentDirection, GameBallRed);
            checkBreakCollapse(ref BlueBallCurrentDirection, GameBallBlue);
        }

        private void moveGameBall(int currentDirection, ref double gameBallTop, ref double gameBallLeft, ref Ellipse gameBall)
        {
            switch (currentDirection)
            {
                case 0:
                    gameBallTop += motionRatio;
                    gameBallLeft += motionRatio;
                    break;

                case 1:
                    gameBallTop += motionRatio;
                    gameBallLeft -= motionRatio;
                    break;

                case 2:
                    gameBallTop -= motionRatio;
                    gameBallLeft -= motionRatio;
                    break;

                case 3:
                    gameBallTop -= motionRatio;
                    gameBallLeft += motionRatio;
                    break;
                default:
                    MessageBox.Show("Ehhh Error occur!!!");
                    break;
            }

            Canvas.SetTop(gameBall, gameBallTop);
            Canvas.SetLeft(gameBall, gameBallLeft);
        }

        private int getDirection(Ellipse _gameBall, int _currentDirection, Rectangle _bottomBrick)
        {
            double _ballLeft = Canvas.GetLeft(_gameBall);
            double _ballTop = Canvas.GetTop(_gameBall);

            if (!checkBottomBreakCollapse(ref _currentDirection, _gameBall, _bottomBrick))
            {
                if (_ballLeft >= MyGameCanvas.Width - _gameBall.Width)
                {
                    if (_currentDirection == 0)
                    {
                        isClockWise = true;
                        return 1;
                    }
                    else
                    {
                        isClockWise = false;
                        return 2;
                    }
                }
                else if (_ballLeft <= 0)
                {
                    if (_currentDirection == 2)
                    {
                        isClockWise = true;
                        return 3;
                    }
                    else
                    {
                        isClockWise = false;
                        return 0;
                    }
                }
                else if (_ballTop <= 0)
                {
                    if (_currentDirection == 3)
                    {
                        isClockWise = true;
                        return 0;
                    }
                    else
                    {
                        isClockWise = false;
                        return 1;
                    }
                }
                else if (_ballTop >= MyGameCanvas.Height - _gameBall.Width)
                {
                    if (_currentDirection == 1)
                    {
                        isClockWise = true;
                        return 2;
                    }
                    else
                    {
                        isClockWise = false;
                        return 3;
                    }
                }
            }

            return _currentDirection;
        }

        private bool checkBottomBreakCollapse(ref int currentDirection, Ellipse _gameBall, Rectangle _bottomBreak)
        {
            double _RTBrick = Canvas.GetTop(_bottomBreak);
            double _RLBrick = Canvas.GetLeft(_bottomBreak);
            double _RRBrick = Canvas.GetLeft(_bottomBreak) + _bottomBreak.Width;
            double _ballBottom = Canvas.GetTop(_gameBall) + _gameBall.Width;
            double _ballLeft = Canvas.GetLeft(_gameBall) + (_gameBall.Width / 2);


            RTBrick = _RTBrick;
            RLBrick = _RLBrick;
             RRBrick = _RRBrick;
            ballBottom =_ballBottom;
             ballLeft = _ballLeft;

            if ((_ballBottom >= _RTBrick && _ballLeft > _RLBrick && _ballLeft < _RRBrick))
            {
                if (currentDirection == 0)
                {
                    mPlayer.Position = new TimeSpan(0);
                    mPlayer.Play();
                    currentDirection = 3;
                }
                else if (currentDirection == 1)
                {
                    mPlayer.Position = new TimeSpan(0);
                    mPlayer.Play();
                    currentDirection = 2;
                }

                return true;
            }
            else if ((_ballBottom >= _RTBrick && (_ballLeft < _RLBrick || _ballLeft > _RRBrick)))
            {
                movingTimer.Stop();
                ice.Stop();
               
                _box.Visibility = Visibility.Visible;
                game_over.Play();
             //   MessageBox.Show("Game Over!!!");
                
                return true;
            }

            return false;
        }

        private bool checkBreakCollapse(ref int currentDirection, Ellipse _gameBall)
        {
            double _ballBottom = Canvas.GetTop(_gameBall) + _gameBall.Width;
            double _ballLeft = Canvas.GetLeft(_gameBall) - (_gameBall.Width / 2);
            double _ballRight = Canvas.GetLeft(_gameBall) + _gameBall.Width;
            double _ballTop = Canvas.GetTop(_gameBall);
            double _ballCenterX = Canvas.GetTop(_gameBall) + (_gameBall.Width / 2);
            double _ballCenterY = Canvas.GetLeft(_gameBall) + (_gameBall.Width / 2);

            var ballCoordinate = getCircularPoints(_gameBall);
            var conflictedBrick = bricks.Where(s => ballCoordinate.Any(p =>
                                                                    p.X >= Canvas.GetLeft(s) &&
                                                                    p.X <= Canvas.GetLeft(s) + s.Width &&
                                                                    p.Y <= Canvas.GetTop(s) + s.Height &&
                                                                    p.Y >= Canvas.GetTop(s)));

            if (conflictedBrick.Count() > 0)
            {
                Rectangle cBrick = conflictedBrick.FirstOrDefault();
                mPlayer.Position = new TimeSpan(0);
                mPlayer.Play();
                if (lastCollapsed == cBrick && skipTick > 0)
                {
                    skipTick--;
                    return false;
                }
                else
                {
                    skipTick = 5;
                    lastCollapsed = cBrick;
                }

                var nearCoordinate1 = ballCoordinate.Where(p =>
                                        p.X >= Canvas.GetLeft(cBrick) &&
                                        p.X <= Canvas.GetLeft(cBrick) + cBrick.Width &&
                                        p.Y <= Canvas.GetTop(cBrick) + cBrick.Height &&
                                        p.Y >= Canvas.GetTop(cBrick));

                var xmin = nearCoordinate1.Min(s => s.X);
                var ymin = nearCoordinate1.Min(s => s.Y);

                var nearCoordinate = xmin < ymin ? nearCoordinate1.OrderByDescending(s => s.X).First() : nearCoordinate1.OrderByDescending(s => s.Y).First();

                if (cBrick == default(Rectangle))
                    MessageBox.Show("Somethign issue");

                if (Canvas.GetTop(cBrick) <= _ballBottom &&        // top
                     Canvas.GetTop(cBrick) + cBrick.Height > _ballBottom &&
                     Canvas.GetLeft(cBrick) <= _ballCenterY &&
                     Canvas.GetLeft(cBrick) + cBrick.Width >= _ballCenterY)
                {
                    isClockWise = currentDirection == 0 ? false : true;
                }
                else if (Canvas.GetTop(cBrick) + cBrick.Height >= _ballCenterX &&             // left
                     Canvas.GetTop(cBrick) < _ballCenterX &&
                     Canvas.GetLeft(cBrick) <= _ballRight &&
                     Canvas.GetLeft(cBrick) + cBrick.Width > _ballRight)
                {
                    isClockWise = currentDirection == 3 ? false : true;
                }
                else if (Canvas.GetTop(cBrick) + cBrick.Height >= _ballTop &&                 // bottom
                                                        Canvas.GetTop(cBrick) < _ballTop &&
                                                        Canvas.GetLeft(cBrick) <= _ballCenterY &&
                                                        Canvas.GetLeft(cBrick) + cBrick.Width >= _ballCenterY)
                {
                    isClockWise = currentDirection == 3 ? true : false;
                }
                else if (Canvas.GetTop(cBrick) + cBrick.Height >= _ballCenterX &&             // right
                                                        Canvas.GetTop(cBrick) < _ballCenterX &&
                                                        Canvas.GetLeft(cBrick) < _ballLeft &&
                                                        Canvas.GetLeft(cBrick) + cBrick.Width >= _ballLeft)
                {
                    isClockWise = currentDirection == 2 ? true : false;
                }

                changeBallDirection(ref currentDirection, _gameBall, cBrick, nearCoordinate);
                int index = bricks.IndexOf(cBrick);

                if (index < 0)
                    MessageBox.Show("Incorrect brick");


                if (brickInfo[index] == "3")
                {
                    cBrick.Fill = Brushes.DarkOrange;
                    brickInfo[index] = "2";
                }
                else if (brickInfo[index] == "2")
                {
                    cBrick.Fill = Brushes.YellowGreen;
                    brickInfo[index] = "1";
                }
                else
                {
                    MyGameCanvas.Children.Remove(cBrick);
                    conflictedBrick.FirstOrDefault().Visibility = System.Windows.Visibility.Collapsed;
                    bricks.Remove(cBrick);
                    brickInfo[index] = "0";
                }

                brickInfo = brickInfo.Where(s => s.ToString() != "0").ToArray();

                if (bricks.Where(s => s.Visibility == System.Windows.Visibility.Visible).Count() == 0)
                {
                    MessageBox.Show(string.Format("You have completed Stage : {0}!!! ", currentGameState + 1));
                    brickGenerator(++currentGameState);
                    setInitialState();
                    return true;
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        private List<coordinates> getCircularPoints(Ellipse gameBall)
        {
            int distance = (int)gameBall.Width / 2;
            double originX = Canvas.GetLeft(gameBall) + distance;
            double originY = Canvas.GetTop(gameBall) - distance;

            List<coordinates> pointLists = new List<coordinates>();
            coordinates point;
            for (int i = 0; i < 360; i = i + 24)
            {
                point = new coordinates();

                point.X = (int)Math.Round(originX + distance * Math.Sin(i));
                point.Y = (int)(gameBall.Width + Math.Round(originY - distance * Math.Cos(i)));
                pointLists.Add(point);
            }

            return pointLists;
        }

        private void changeBallDirection(ref int _currentDirection, Ellipse _gameBall, Rectangle _crashBrick, coordinates nearCoordinate)
        {
            int hitAt;
            int left = (int)(nearCoordinate.X - Canvas.GetLeft(_crashBrick));
            int right = (int)(nearCoordinate.X - (Canvas.GetLeft(_crashBrick) + _crashBrick.Width));
            int top = (int)(nearCoordinate.Y - Canvas.GetTop(_crashBrick));
            int bottom = (int)(nearCoordinate.Y - (Canvas.GetTop(_crashBrick) + _crashBrick.Height));

            int[] values = { Math.Abs(left), Math.Abs(right), Math.Abs(top), Math.Abs(bottom) };
            Array.Sort(values);

            if (values[0] == left)
                hitAt = 3;
            else if (values[0] == right)
                hitAt = 1;
            else if (values[0] == top)
                hitAt = 0;
            else
                hitAt = 2;

            switch (_currentDirection)
            {
                case 0:

                    if (hitAt == 3)
                        _currentDirection = 1;
                    else if (hitAt == 0)
                        _currentDirection = 3;
                    else if (top < left)
                        _currentDirection = 3;
                    else
                        _currentDirection = 1;

                    break;

                case 1:

                    if (hitAt == 1)
                        _currentDirection = 0;
                    else if (hitAt == 0)
                        _currentDirection = 2;
                    else if (top < right)
                        _currentDirection = 2;
                    else
                        _currentDirection = 0;
                    break;

                case 2:

                    if (hitAt == 2)
                        _currentDirection = 1;
                    else if (hitAt == 1)
                        _currentDirection = 3;
                    else if (bottom < right)
                        _currentDirection = 1;
                    else
                        _currentDirection = 3;
                    break;

                case 3:

                    if (hitAt == 2)
                        _currentDirection = 0;
                    else if (hitAt == 3)
                        _currentDirection = 2;
                    else if (bottom < right)
                        _currentDirection = 0;
                    else
                        _currentDirection = 2;
                    break;
            }
        }

        private void brickGenerator(int currentStage)
        {
            changeBackgroundImage();
            Rectangle rct;

            try
            {
                bricks.Clear();

                if (stagesInfo.Length <= currentStage)
                {
                    movingTimer.Stop();
                    MessageBox.Show("You have completed all the Stage. Congratulation!!!");
                }
                else
                {
                    brickInfo = stagesInfo[currentStage].Split(',');

                    for (int i = 1; i <= 10; i++)
                    {
                        for (int j = 1; j <= 10; j++)
                        {
                            rct = new Rectangle();
                            rct.Opacity = 1;
                            if (!string.IsNullOrWhiteSpace(brickInfo[(j + ((i - 1) * 10)) - 1]))
                            {
                                int brickType = Convert.ToInt16(brickInfo[(j + ((i - 1) * 10)) - 1]);

                                switch (brickType)
                                {
                                    case 0:
                                        break;

                                    case 1:
                                        rct.Fill = Brushes.YellowGreen;
                                        break;

                                    case 2:
                                        rct.Fill = Brushes.DarkOrange;
                                        break;

                                    case 3:
                                        rct.Fill = Brushes.Khaki;
                                        break;
                                }

                                rct.Height = 25;
                                rct.Width = 60;
                                rct.Stroke = Brushes.Black;
                                rct.RadiusX = 1;
                                rct.RadiusY = 1;
                                rct.StrokeThickness = 1;
                                Canvas.SetLeft(rct, (j * 60) - 30);
                                Canvas.SetTop(rct, (i * 25));
                                bricks.Insert((j + ((i - 1) * 10)) - 1, rct);
                                MyGameCanvas.Children.Insert(0, rct);
                            }
                            else
                            {
                                rct.Visibility = System.Windows.Visibility.Collapsed;
                                bricks.Add(rct);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void clearCanvas()
        {
            foreach (Rectangle item in bricks)
            {
                item.Visibility = System.Windows.Visibility.Collapsed;
            }

            bricks.Clear();
        }

        private void setInitialState()
        {
            _box.Visibility = Visibility.Hidden;
            Canvas.SetLeft(GameBallRed, 40);
            Canvas.SetTop(GameBallRed, 470);
            Canvas.SetLeft(rectangleRed, 0);

            Canvas.SetLeft(GameBallBlue, 590);
            Canvas.SetTop(GameBallBlue, 470);
            Canvas.SetLeft(rectangleBlue, 550);

            RedGameBallTop = Canvas.GetTop(GameBallRed);
            BlueGameBallTop = Canvas.GetTop(GameBallBlue);
            RedGameBallLeft = Canvas.GetLeft(GameBallRed);
            BlueGameBallLeft = Canvas.GetLeft(GameBallBlue);

            RedBallCurrentDirection = 3;
            BlueBallCurrentDirection = 2;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (movingTimer.IsEnabled && e.Key == Key.Space)
            {
                ice.Pause();
                movingTimer.Stop();
                Pause p = new Pause(this);
                p.ShowDialog();
            }

            switch (e.Key)
            {
                case Key.Escape:
                    ice.Pause();
                    movingTimer.Stop();
                   
                   var res= MessageBox.Show("Do you really want to Close Game ?", "Confirmation", MessageBoxButton.YesNo);
                   if (res.ToString() == "Yes")
                   {
                       Close();
                   }
                   else
                       movingTimer.Start();
                      ice.Play();
                   break;
                case Key.LeftCtrl:
                   ice.Play();
                    movingTimer.Start();
                    setInitialState();
                    clearCanvas();
                    if (currentGameState<6)
                    brickGenerator(++currentGameState);
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("Only 6 Level Til Now :)");
                        currentGameState = 1;
                    }
                    break;

                    //if (!(ballBottom >= RTBrick && (ballLeft < RLBrick || ballLeft > RRBrick)))
                    
                case Key.Enter:
                    Touch.FrameReported += new TouchFrameEventHandler(Touch_FrameReportedRed);
                    ice.Play();
                 movingTimer.Start();
                    currentGameState = 1;
                    setInitialState();

                    clearCanvas();
                    brickGenerator(currentGameState);
                    break;

                case Key.Right:
                    var rightBlue = Canvas.GetLeft(rectangleBlue);
                    if (rightBlue < 550)
                    {
                        Canvas.SetLeft(rectangleBlue, rightBlue + 20);
                    }
                   break;

                case Key.D:
                    var rightRed = Canvas.GetLeft(rectangleRed);
                    if (rightRed < 550)
                    {
                        Canvas.SetLeft(rectangleRed, rightRed + 20);
                    }
                    break;

               
                case Key.Left:
                    var leftBlue = Canvas.GetLeft(rectangleBlue);
                    if (leftBlue > 0)
                    {
                        Canvas.SetLeft(rectangleBlue, leftBlue - 20);
                    }
                    break;

               case Key.A:
                    var leftRed = Canvas.GetLeft(rectangleRed);
                    if (leftRed > 0)
                    {
                        Canvas.SetLeft(rectangleRed, leftRed - 20);
                    }
                    break;
                    
                case Key.F:
                    Help h1 = new Help();
                    h1.Show();
                    break;

               
            }
        }

        public void startApp()
        {
            movingTimer.Start();
        }
    }
}