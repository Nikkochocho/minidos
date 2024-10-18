clear()

print('OpenAI - Chat GPT\n')

question = get_args()
print('Pergunta ==> '..question..'\n')

response = ask_gpt(question)
print('Resposta ==> '..response..'\n')
