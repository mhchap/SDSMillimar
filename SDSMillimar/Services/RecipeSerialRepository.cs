using SDSMillimar.Models;
using SDSMillimar.Utils;
using System;
using System.Data.Entity;
using System.Threading.Tasks;

namespace SDSMillimar.Services
{
    internal class RecipeSerialRepository
    {
        public async Task<int> GetNextSerial(string recipeNo)
        {
            var today = DateTime.Today;
            using (var db = new AppDbContext())
            {
                try
                {
                    var serial = await db.RecipeSerials.FirstOrDefaultAsync(x => x.RecipeNo == recipeNo && x.SerialDate == today);

                    if (serial == null)
                    {
                        serial = new RecipeSerial
                        {
                            RecipeNo = recipeNo,
                            SerialDate = today,
                            CurrentValue = 1
                        };

                        db.RecipeSerials.Add(serial);
                    }
                    else
                    {
                        serial.CurrentValue++;
                    }

                    await db.SaveChangesAsync();

                    return serial.CurrentValue;
                }
                catch (Exception)
                {

                    return -1;
                }
            }

        }
    }
}
