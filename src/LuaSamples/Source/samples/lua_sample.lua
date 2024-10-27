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
