clear()

print('OpenAI - Chat GPT\n')

question = get_args()
print('Pergunta ==> '..question)

response = ask_gpt(question)
print('Resposta ==> '..response)

print()
