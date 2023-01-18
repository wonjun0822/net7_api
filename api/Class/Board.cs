namespace net7_api.Class
{
    public class Board
    {
        public int idx { get; set; }
        public int board_idx { get; set; }

        public string title { get; set; }
        public string content { get; set; }
        public string create_id { get; set; }
        public DateTime create_time { get; set; }
        public string update_id { get; set; }
        public DateTime update_time { get; set; }
    }
}