--[[
  MiniDOS
  Copyright (C) 2024  Lara H. Ferreira and others.
  This program is free software: you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by
  the Free Software Foundation, either version 3 of the License, or
  (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.
 
  You should have received a copy of the GNU General Public License
  along with this program.  If not, see <http://www.gnu.org/licenses/>.
]]--


local base_path = string.match(get_current_path(), '^(.-)[^/\\]*$')
package.path = string.format("%s;%s?.lua", package.path, base_path)

require("frames_hi_res")

enable_auto_carriage_return(false)

play(get_current_path().."bad_apple.wav")
clear()

for i = 0, #frames do
   home()
   print(frames[i])
   wait(23)
end