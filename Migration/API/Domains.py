from flask import Flask
from flask import request
import psycopg2
import json

app = Flask(__name__)

try:
	conn = psycopg2.connect("dbname='schef' user='postgres' host='localhost' password=''")
	print "DB Connection Complete"
except:
	print "I am unable to connect to the database"

cur = conn.cursor()
#cur.execute("CREATE TABLE employee(id serial primary key not null, f_name varchar, l_name varchar, age int)")
#cur.execute("INSERT INTO employee (f_name) VALUES (%s, %s)", ("Daniel", "Baah"))
#sql1 = "INSERT INTO employee (f_name, l_name, age) VALUES (%s, %s, %s)"
sql = "SELECT * FROM domain_variable where id = %s"
#data = ("Deion", "Sanders", 14, )

#city = "Seattle"

#str(cur.description[i][0])

def get_domain_by_id (id):
	i = -1
	data = (id, )
	cur.execute(sql, data)

	#mydict = dict(str(cur.fetchall()))
	
	obj = cur.fetchone()
	cc = " "
	index = 0

	domain = {}

	for item in obj:
		domain[str(cur.description[index][0])] = str(item)
		#response.append({str(cur.description[index][0]): str(item)})
		#cc += "\"" + str(cur.description[index][0]) + "\": " + str(item) + ", "
		index+=1



	#return "{ " + cc + " } "
	return json.dumps(domain)

#cur.execute(sql, data)

#conn.commit()

@app.route('/hello')
def hello():
	return get_domain_by_id(3)

if __name__ == '__main__':
    app.run(debug=True)

cur.close()
conn.close()