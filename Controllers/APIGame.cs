using Azure;
using GAM106_LAB.Data;
using GAM106_LAB.DTO;
using GAM106_LAB.Models;
using GAM106_LAB.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GAM106_LAB.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class APIGame : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        protected ApiResponse<object> _response;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;

        public APIGame(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
            _response = new ApiResponse<object>();
        }

        [HttpGet]
        public ActionResult<ApiResponse<GameModel>> Get()
        {
            var game106 = new GameModel
            {
                CourseName = "Backend Development",
                CourseCode = "GAME2003",
                Name = "Hoang",
                Class = "GAM106"
            };

            var response = new ApiResponse<GameModel>
            {
                Status = 1,
                Message = "Success",
                Data = game106
            };
            return Ok(response);
        }

        [HttpGet("GetAllGameLevel")]
        public async Task<IActionResult> GetAllGameLevel()
        {
            try
            {
                var gameLevels = await _context.GameLevels.ToListAsync();
                _response.Status = 1;
                _response.Data = gameLevels;
                _response.Message = "Success";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.Status = 0;
                _response.Message = ex.Message;
                return BadRequest(_response);
            }
        }

        [HttpGet("GetQuestionGame")]
        public async Task<IActionResult> GetQuestionGame(int levelId)
        {
            try
            {
                var questions = await _context.Questions.Where(q => q.LevelId == levelId).ToListAsync();
                _response.Status = 1;
                _response.Data = questions;
                _response.Message = "Success";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.Status = 0;
                _response.Message = ex.Message;
                return BadRequest(_response);
            }
        }

        // New: GET with path parameter to retrieve questions by level
        [HttpGet("GetAllQuestionGameByLevel/{levelId}")]
        public async Task<IActionResult> GetAllQuestionGameByLevel([FromRoute] int levelId)
        {
            try
            {
                var questions = await _context.Questions.Where(q => q.LevelId == levelId).ToListAsync();
                _response.Status = 1;
                _response.Data = questions;
                _response.Message = "Success";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.Status = 0;
                _response.Message = ex.Message;
                return BadRequest(_response);
            }
        }

        [HttpGet("GetAllRegions")]
        public async Task<IActionResult> GetAllRegions()
        {
            try
            {
                var regions = await _context.Regions.ToListAsync();
                _response.Status = 1;
                _response.Data = regions;
                _response.Message = "Success";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.Status = 0;
                _response.Message = ex.Message;
                return BadRequest(_response);
            }
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] List<RegisterDTO> registerDTOs)
        {
            try
            {
                if (registerDTOs == null || !registerDTOs.Any())
                {
                    _response.Status = 0;
                    _response.Message = "No users provided.";
                    return BadRequest(_response);
                }

                var results = new List<object>();

                foreach (var registerDTO in registerDTOs)
                {
                    if (registerDTO == null || string.IsNullOrWhiteSpace(registerDTO.Email) || string.IsNullOrWhiteSpace(registerDTO.Password))
                    {
                        results.Add(new { Email = registerDTO?.Email, Success = false, Errors = new[] { "Email and Password are required." } });
                        continue;
                    }

                    // validate region, fallback to region 1 if not exists
                    var regionExists = await _context.Regions.AnyAsync(r => r.RegionId == registerDTO.RegionId);
                    if (!regionExists)
                    {
                        results.Add(new { Email = registerDTO.Email, Success = false, Errors = new[] { $"Region with id {registerDTO.RegionId} does not exist." } });
                        continue;
                    }

                    var user = new ApplicationUser
                    {
                        UserName = registerDTO.Email,
                        Email = registerDTO.Email,
                        FullName = registerDTO.Name,
                        RegionId = registerDTO.RegionId,
                        ProfilePictureUrl = registerDTO.LinkAvatar
                    };

                    var result = await _userManager.CreateAsync(user, registerDTO.Password);
                    if (result.Succeeded)
                    {
                        results.Add(new { Email = registerDTO.Email, Success = true, UserId = user.Id });
                    }
                    else
                    {
                        results.Add(new { Email = registerDTO.Email, Success = false, Errors = result.Errors.Select(e => e.Description) });
                    }
                }

                _response.Status = 1;
                _response.Message = "Batch registration completed.";
                _response.Data = results;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.Status = 0;
                _response.Message = ex.Message;
                _response.Data = ex.Message;
                return BadRequest(_response);
            }
        }

        [HttpPost("SaveResult")]
        public async Task<IActionResult> SaveResult([FromBody] JsonElement body)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = JsonNumberHandling.AllowReadingFromString
                };

                SaveResultDTO dto = null;

                if (body.ValueKind == JsonValueKind.Object && body.TryGetProperty("dto", out var inner))
                {
                    dto = JsonSerializer.Deserialize<SaveResultDTO>(inner.GetRawText(), options);
                }
                else
                {
                    dto = JsonSerializer.Deserialize<SaveResultDTO>(body.GetRawText(), options);
                }

                if (dto == null || string.IsNullOrWhiteSpace(dto.UserId))
                {
                    _response.Status = 0;
                    _response.Message = "Invalid result data.";
                    return BadRequest(_response);
                }

                var levelResult = new LevelResult
                {
                    UserId = dto.UserId,
                    LevelId = dto.LevelId,
                    Score = dto.Score,
                    CompletionDate = DateOnly.FromDateTime(dto.CompletionDate)
                };

                _context.LevelResults.Add(levelResult);
                await _context.SaveChangesAsync();

                _response.Status = 1;
                _response.Message = "Result saved successfully.";
                _response.Data = levelResult;
                return Ok(_response);
            }
            catch (JsonException jex)
            {
                _response.Status = 0;
                _response.Message = "Invalid JSON format: " + jex.Message;
                _response.Data = jex.Message;
                return BadRequest(_response);
            }
            catch (Exception ex)
            {
                _response.Status = 0;
                _response.Message = ex.Message;
                _response.Data = ex.Message;
                return BadRequest(_response);
            }
        }

        [HttpGet("Rating")]
        public async Task<IActionResult> Rating(int top = 10)
        {
            try
            {
                var ratings = await _context.LevelResults
                    .GroupBy(r => r.UserId)
                    .Select(g => new
                    {
                        UserId = g.Key,
                        TotalScore = g.Sum(x => x.Score),
                        AverageScore = g.Average(x => x.Score),
                        Attempts = g.Count()
                    })
                    .OrderByDescending(x => x.TotalScore)
                    .Take(top)
                    .ToListAsync();

                var userIds = ratings.Select(r => r.UserId).ToList();
                var users = await _userManager.Users.Where(u => userIds.Contains(u.Id)).ToListAsync();

                var result = ratings.Select(r => new
                {
                    r.UserId,
                    FullName = users.FirstOrDefault(u => u.Id == r.UserId)?.FullName ?? users.FirstOrDefault(u => u.Id == r.UserId)?.Email,
                    r.TotalScore,
                    AverageScore = Math.Round(r.AverageScore, 2),
                    r.Attempts
                });

                _response.Status = 1;
                _response.Message = "Success";
                _response.Data = result;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.Status = 0;
                _response.Message = ex.Message;
                return BadRequest(_response);
            }
        }

        // New: Get user information by userId
        [HttpGet("GetUserInformation/{userId}")]
        public async Task<IActionResult> GetUserInformation([FromRoute] string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _response.Status = 0;
                    _response.Message = "UserId is required.";
                    return BadRequest(_response);
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _response.Status = 0;
                    _response.Message = "User not found.";
                    return NotFound(_response);
                }

                var region = await _context.Regions.FindAsync(user.RegionId);
                var totalScore = await _context.LevelResults.Where(l => l.UserId == userId).SumAsync(l => (int?)l.Score) ?? 0;
                var attempts = await _context.LevelResults.CountAsync(l => l.UserId == userId);

                var data = new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.FullName,
                    user.ProfilePictureUrl,
                    Region = region?.RegionName,
                    TotalScore = totalScore,
                    Attempts = attempts
                };

                _response.Status = 1;
                _response.Message = "Success";
                _response.Data = data;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.Status = 0;
                _response.Message = ex.Message;
                _response.Data = ex.Message;
                return BadRequest(_response);
            }
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(loginRequest.Email);
                if (user != null && await _userManager.CheckPasswordAsync(user, loginRequest.Password))
                {
                    _response.Status = 1;
                    _response.Message = "Login successful";
                    _response.Data = new { user.Id, user.UserName, user.Email };
                    return Ok(_response);
                }
                else
                {
                    _response.Status = 0;
                    _response.Message = "Invalid email or password";
                    return Unauthorized(_response);
                }
            }
            catch (Exception ex)
            {
                _response.Status = 0;
                _response.Message = ex.Message;
                return BadRequest(_response);
            }
        }

        // New: return all users (for quick local inspection)
        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userManager.Users
                    .Where(u => !u.IsDeleted)
                    .Select(u => new
                    {
                        u.Id,
                        u.UserName,
                        u.Email,
                        u.FullName,
                        u.RegionId,
                        u.ProfilePictureUrl,
                        u.IsDeleted
                    })
                    .ToListAsync();

                _response.Status = 1;
                _response.Message = "Success";
                _response.Data = users;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.Status = 0;
                _response.Message = ex.Message;
                return BadRequest(_response);
            }
        }
        [HttpPut("ChangeUserPassword")]
        public async Task<IActionResult> ChangeUserPassword([FromBody] ChangePasswordDTO dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.UserId) || string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                _response.Status = 0;
                _response.Message = "Invalid request.";
                return BadRequest(_response);
            }

            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
            {
                _response.Status = 0;
                _response.Message = "User not found.";
                return NotFound(_response);
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);

            if (!result.Succeeded)
            {
                _response.Status = 0;
                _response.Message = string.Join("; ", result.Errors.Select(e => e.Description));
                return BadRequest(_response);
            }

            _response.Status = 1;
            _response.Message = "Password changed successfully.";
            return Ok(_response);
        }

        [HttpPut("UpdateUserInformation")]
        public async Task<IActionResult> UpdateUserInformation([FromForm] UpdateUserInformationDTO dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.UserId))
            {
                _response.Status = 0;
                _response.Message = "Invalid request.";
                return BadRequest(_response);
            }

            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
            {
                _response.Status = 0;
                _response.Message = "User not found.";
                return NotFound(_response);
            }

            user.FullName = dto.FullName;
            user.RegionId = dto.RegionId;

            if (dto.Avatar != null && dto.Avatar.Length > 0)
            {
                var ext = Path.GetExtension(dto.Avatar.FileName);
                var fileName = $"{dto.UserId}{ext}";
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
                Directory.CreateDirectory(folder);
                var filePath = Path.Combine(folder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.Avatar.CopyToAsync(stream);
                }

                // set URL or relative path
                user.ProfilePictureUrl = $"/uploads/avatars/{fileName}";
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                _response.Status = 0;
                _response.Message = string.Join("; ", updateResult.Errors.Select(e => e.Description));
                return BadRequest(_response);
            }

            _response.Status = 1;
            _response.Message = "User information updated successfully.";
            _response.Data = user;
            return Ok(_response);
        }

        [HttpDelete("DeleteUser/{userId}")]
        public async Task<IActionResult> DeleteUser([FromRoute] string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _response.Status = 0;
                    _response.Message = "User not found.";
                    _response.Data = null;
                    return BadRequest(_response);
                }
                // soft delete via UserManager to ensure identity store handles concurrency and updates
                user.IsDeleted = true;
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    _response.Status = 0;
                    _response.Message = string.Join("; ", updateResult.Errors.Select(e => e.Description));
                    _response.Data = null;
                    return BadRequest(_response);
                }

                _response.Status = 1;
                _response.Message = "User deleted successfully.";
                _response.Data = null;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.Status = 0;
                _response.Message = ex.Message;
                _response.Data = ex.Message;
                return BadRequest(_response);
            }
        }

        [HttpPost("ForgetPassword")]
        public async Task<IActionResult> ForgetPassword(string Email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(Email);
                if (user == null)
                {
                    _response.Status = 0;
                    _response.Message = "User not found.";
                    _response.Data = null;
                    return NotFound(_response);
                }
                Random random = new();
                string OTP = random.Next(100000, 999999).ToString();
                user.OTP = OTP;
                await _userManager.UpdateAsync(user);
                await _context.SaveChangesAsync();
                string subject = "Password Reset OTP" + Email;
                string message = "Your OTP for password reset is: " + OTP;
                await _emailService.SendEmailAsync(Email, subject, message);
                _response.Status = 1;
                _response.Message = "OTP sent to email.";
                _response.Data = "email sent to" + Email;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.Status = 0;
                _response.Message = ex.Message;
                return BadRequest(_response);
            }
        }

        [HttpPost("CheckOTP")]
        public async Task<IActionResult> CheckOTP(CheckOTPDTO checkOTPDTO)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(checkOTPDTO.Email);
                if (user == null)
                {
                    _response.Status = 0;
                    _response.Message = "User not found.";
                    _response.Data = null;
                    return NotFound(_response);
                }
                var stringOTP = Convert.ToInt32(checkOTPDTO.OTP).ToString();
                if (user.OTP == stringOTP)
                {
                    _response.Status = 1;
                    _response.Message = "OTP is valid.";
                    _response.Data = user.Email;
                    return Ok(_response);
                }
                else
                {
                    _response.Status = 0;
                    _response.Message = "Invalid OTP.";
                    _response.Data = null;
                    return BadRequest(_response);
                }
            }
            catch (Exception ex)
            {
                _response.Status = 0;
                _response.Message = ex.Message;
                return BadRequest(_response);
            }
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDTO resetPasswordDTO)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(resetPasswordDTO.Email);
                if (user == null)
                {
                    _response.Status = 0;
                    _response.Message = "User not found.";
                    _response.Data = null;
                    return NotFound(_response);
                }
                var stringOTP = Convert.ToInt32(resetPasswordDTO.OTP).ToString();
                if (user.OTP == stringOTP)
                {
                    DateTime now = DateTime.Now;
                    user.OTP = $"{stringOTP}_used_" + now.ToString("yyyy/MM/dd_HH:mm:ss");
                    var passwordHashr = new PasswordHasher<IdentityUser>();
                    user.PasswordHash = passwordHashr.HashPassword(user, resetPasswordDTO.NewPassword);
                    var result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        _response.Status = 1;
                        _response.Message = "Password reset successfully.";
                        _response.Data = null;
                        return Ok(_response);
                    }
                    else
                    {
                        _response.Status = 0;
                        _response.Message = string.Join("; ", result.Errors.Select(e => e.Description));
                        _response.Data = null;
                        return BadRequest(_response);
                    }
                }
                else
                {
                    _response.Status = 0;
                    _response.Message = "Invalid OTP.";
                    _response.Data = null;
                    return BadRequest(_response);
                }
            }
            catch (Exception ex)
            {
                _response.Status = 0;
                _response.Message = ex.Message;
                return BadRequest(_response);
            }
        }
    }
}
