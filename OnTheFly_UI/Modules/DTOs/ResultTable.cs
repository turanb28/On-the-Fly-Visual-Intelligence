using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnTheFly_UI.Modules.DTOs
{
    public class ResultTable
    {
        public int Index { get; set; } = 0;
        public List<ResultTableItem> Items { get; set; } = new List<ResultTableItem>();
        public ResultTable()
        {
        }
        public ResultTable(List<ResultTableItem> items)
        {
            Items = items;
        }

        public void AddItem(ResultTableItem item)
        {
            var existingItem = Items.FirstOrDefault(i => i.Name == item.Name);
            if (existingItem != null)
            {
                existingItem.Count += item.Count;
            }
            else
            {
                Items.Add(item);
            }
        }

        public void Clear()
        {
            Items.Clear();
        }
    }
}
