type TicketSort = 'title' | 'developer' | 'status' | 'priority' | 'date'
type TicketOrder = 'asc' | 'desc'
type Ticket = {
  id: number
  title: string
  developer: string
  status: string
  priority: string
  date: string
}

class TicketsTable {
  tickets: Ticket[]
  container: HTMLElement
  page: number
  limit: number
  sortBy: TicketSort
  order: TicketOrder

  constructor(
    tickets: Ticket[],
    container?: HTMLElement,
    page?: number,
    limit?: number,
    sortBy?: TicketSort,
    order?: TicketOrder
  ) {
    this.tickets = tickets ?? [],
    this.container = container ?? document.querySelector('[data-container="tickets"]') as HTMLElement,
    this.page = page ?? 1,
    this.limit = limit ?? 10,
    this.sortBy = sortBy ?? 'title',
    this.order = order ?? 'asc'
  }

  async init() {
    this.getTickets()

    this.container.addEventListener('click', async (event) => {
      const header = (event.target as HTMLElement).closest('[data-sortable]')
      if (header) {
        if (header.getAttribute('data-order') === this.order)
          this.order =
            header.getAttribute('data-order') === 'asc' ? 'desc' : 'asc'

        this.page =
          parseInt(header.getAttribute('data-page') as string) ?? this.page
        this.limit =
          parseInt(header.getAttribute('data-limit') as string) ?? this.limit
        this.sortBy = (header.getAttribute('data-sort') as TicketSort) ?? this.sortBy

        this.getTickets()
      }
    })
  }

  async getTickets() {
    try {
      const url = `/Tickets/SortTickets?page=${this.page}&sortBy=${this.sortBy}&order=${this.order}&limit=${this.limit}`

      const response = await fetch(url, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(this.tickets),
      })

      if (!response.ok) {
        throw new Error(`Error loading tickets table: ${response.statusText}`)
      }

      const html = await response.text()
    
      this.container.innerHTML = html

      const header = this.container.querySelector(
        `[data-sort="${this.sortBy}"]`
      )!
      header.setAttribute('data-order', this.order as TicketOrder)

      header.classList.add('active')

      const arrow = header.querySelector('.order')

      arrow?.classList.add(this.order as TicketOrder)
    } catch (error) {
      console.error(error)
    }
  }
}
