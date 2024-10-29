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

print('Lua Sample')

args = get_args()

print('Command line arguments '.. args)

x = 3
y = 4
z = x + y
print('z = ' .. z)

for i=1,10
do
	print(i)
	wait(1000)

	if(i == 5) then
		clear()
	end
end

clear()
